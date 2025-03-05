using IFRS16_Backend.enums;
using IFRS16_Backend.Helper;
using IFRS16_Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace IFRS16_Backend.Services.JournalEntries
{
    public class JournalEntriesService(ApplicationDbContext context) : IJournalEntriesService
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<List<JournalEntryTable>> PostJEForLease(LeaseFormData leaseSpecificData, List<LeaseLiabilityTable> leaseLiability, List<ROUScheduleTable> rouSchedule)
        {
            int startTableDates = leaseSpecificData.Annuity == "advance" ? 1 : 0;
            LeaseLiabilityTable leaseMustField = leaseLiability[0];
            ROUScheduleTable respectiveROU = rouSchedule[0];
            List<JournalEntryTable> JEFinalTable =
            [
                    new JournalEntryTable
                    {
                        JE_Date = respectiveROU.ROU_Date,
                        Particular = "ROU Asset",
                        Debit = (decimal)(respectiveROU.Opening - (leaseSpecificData.IDC ?? 0)),
                        Credit = 0,
                        LeaseId = leaseSpecificData.LeaseId
                    },
                    new JournalEntryTable
                    {
                        JE_Date = leaseMustField.LeaseLiability_Date,
                        Particular = "Lease Liability",
                        Debit = 0,
                        Credit = (decimal)(leaseMustField.Opening - leaseMustField.Payment),
                        LeaseId = leaseSpecificData.LeaseId
                    }
,
            ];

            // Handle payment
            if (leaseMustField.Payment > 0)
            {
                JEFinalTable.Add(new JournalEntryTable
                {
                    JE_Date = leaseMustField.LeaseLiability_Date,
                    Particular = "Bank",
                    Debit = 0,
                    Credit = (decimal)leaseMustField.Payment,
                    LeaseId = leaseSpecificData.LeaseId
                });
            }

            // Handle IDC
            if (leaseSpecificData.IDC.HasValue && leaseSpecificData.IDC != 0)
            {
                JEFinalTable.Add(new JournalEntryTable
                {
                    JE_Date = respectiveROU.ROU_Date,
                    Particular = "ROU Asset",
                    Debit = (decimal)leaseSpecificData.IDC,
                    Credit = 0,
                    LeaseId = leaseSpecificData.LeaseId
                });

                JEFinalTable.Add(new JournalEntryTable
                {
                    JE_Date = respectiveROU.ROU_Date,
                    Particular = "Bank (IDC)",
                    Debit = 0,
                    Credit = (decimal)leaseSpecificData.IDC,
                    LeaseId = leaseSpecificData.LeaseId
                });
            }

            for (int i = startTableDates; i < leaseLiability.Count; i++)
            {
                LeaseLiabilityTable leaseliabilityData = leaseLiability[i];
                ROUScheduleTable rouData = rouSchedule[i];
                double threshold = 1e-7; // Adjust this based on precision requirements
                double exhangeGainLossChecking = Math.Abs((double)leaseliabilityData?.Exchange_Gain_Loss) < threshold ? 0 : (double)leaseliabilityData?.Exchange_Gain_Loss;

                // Create and push interest and lease interest journal entries
                JEFinalTable.Add(new JournalEntryTable
                {
                    JE_Date = leaseliabilityData.LeaseLiability_Date,
                    Particular = "Interest Expense",
                    Debit = (decimal)leaseliabilityData.Interest,
                    Credit = 0,
                    LeaseId = leaseSpecificData.LeaseId
                });

                JEFinalTable.Add(new JournalEntryTable
                {
                    JE_Date = leaseliabilityData.LeaseLiability_Date,
                    Particular = "Lease Liability",
                    Debit = 0,
                    Credit = (decimal)leaseliabilityData.Interest,
                    LeaseId = leaseSpecificData.LeaseId
                });

                if (exhangeGainLossChecking != 0 && leaseliabilityData?.Exchange_Gain_Loss != null)
                {
                    JEFinalTable.Add(new JournalEntryTable
                    {
                        JE_Date = leaseliabilityData.LeaseLiability_Date,
                        Particular = "Exchange Gain/Loss",
                        Debit = (decimal)leaseliabilityData?.Exchange_Gain_Loss < 0 ? (decimal)leaseliabilityData.Exchange_Gain_Loss : 0,
                        Credit = (decimal)leaseliabilityData?.Exchange_Gain_Loss > 0 ? (decimal)leaseliabilityData.Exchange_Gain_Loss : 0,
                        LeaseId = leaseSpecificData.LeaseId
                    });
                    JEFinalTable.Add(new JournalEntryTable
                    {
                        JE_Date = leaseliabilityData.LeaseLiability_Date,
                        Particular = "Lease Liability",
                        Debit = (decimal)leaseliabilityData?.Exchange_Gain_Loss > 0 ? (decimal)leaseliabilityData.Exchange_Gain_Loss : 0,
                        Credit = (decimal)leaseliabilityData?.Exchange_Gain_Loss < 0 ? (decimal)leaseliabilityData.Exchange_Gain_Loss : 0,
                        LeaseId = leaseSpecificData.LeaseId
                    });
                }


                // Create and push amortization and ROU journal entries
                JEFinalTable.Add(new JournalEntryTable
                {
                    JE_Date = rouData.ROU_Date,
                    Particular = "Amortization Expense",
                    Debit = (decimal)rouData.Amortization,
                    Credit = 0,
                    LeaseId = leaseSpecificData.LeaseId
                });

                JEFinalTable.Add(new JournalEntryTable
                {
                    JE_Date = rouData.ROU_Date,
                    Particular = "ROU Asset",
                    Debit = 0,
                    Credit = (decimal)rouData.Amortization,
                    LeaseId = leaseSpecificData.LeaseId
                });

                // Handle payment entries if payment is greater than 0
                if (leaseliabilityData.Payment > 0)
                {
                    JEFinalTable.Add(new JournalEntryTable
                    {
                        JE_Date = leaseliabilityData.LeaseLiability_Date,
                        Particular = "Lease Liability",
                        Debit = (decimal)leaseliabilityData.Payment,
                        Credit = 0,
                        LeaseId = leaseSpecificData.LeaseId
                    });

                    JEFinalTable.Add(new JournalEntryTable
                    {
                        JE_Date = leaseliabilityData.LeaseLiability_Date,
                        Particular = "Bank",
                        Debit = 0,
                        Credit = (decimal)leaseliabilityData.Payment,
                        LeaseId = leaseSpecificData.LeaseId
                    });
                }
            }

            _context.JournalEntries.AddRange(JEFinalTable);
            await _context.SaveChangesAsync();

            return JEFinalTable;
        }

        public async Task<List<FC_JournalEntryTable>> PostJEForLeaseforFC(LeaseFormData leaseSpecificData, List<FC_LeaseLiabilityTable> fc_leaseLiability, List<FC_ROUScheduleTable> fc_rouSchedule)
        {
            int startTableDates = leaseSpecificData.Annuity == "advance" ? 1 : 0;
            FC_LeaseLiabilityTable leaseMustField = fc_leaseLiability[0];
            FC_ROUScheduleTable respectiveROU = fc_rouSchedule[0];
            List<FC_JournalEntryTable> JEFinalTable =
            [
                    new FC_JournalEntryTable
                    {
                        JE_Date = respectiveROU.ROU_Date,
                        Particular = "ROU Asset",
                        Debit = (decimal)(respectiveROU.Opening - (leaseSpecificData.IDC ?? 0)),
                        Credit = 0,
                        LeaseId = leaseSpecificData.LeaseId
                    },
                    new FC_JournalEntryTable
                    {
                        JE_Date = leaseMustField.LeaseLiability_Date,
                        Particular = "Lease Liability",
                        Debit = 0,
                        Credit = (decimal)(leaseMustField.Opening - leaseMustField.Payment),
                        LeaseId = leaseSpecificData.LeaseId
                    }
,
            ];

            // Handle payment
            if (leaseMustField.Payment > 0)
            {
                JEFinalTable.Add(new FC_JournalEntryTable
                {
                    JE_Date = leaseMustField.LeaseLiability_Date,
                    Particular = "Bank",
                    Debit = 0,
                    Credit = (decimal)leaseMustField.Payment,
                    LeaseId = leaseSpecificData.LeaseId
                });
            }

            // Handle IDC
            if (leaseSpecificData.IDC.HasValue && leaseSpecificData.IDC != 0)
            {
                JEFinalTable.Add(new FC_JournalEntryTable
                {
                    JE_Date = respectiveROU.ROU_Date,
                    Particular = "ROU Asset",
                    Debit = (decimal)leaseSpecificData.IDC,
                    Credit = 0,
                    LeaseId = leaseSpecificData.LeaseId
                });

                JEFinalTable.Add(new FC_JournalEntryTable
                {
                    JE_Date = respectiveROU.ROU_Date,
                    Particular = "Bank (IDC)",
                    Debit = 0,
                    Credit = (decimal)leaseSpecificData.IDC,
                    LeaseId = leaseSpecificData.LeaseId
                });
            }

            for (int i = startTableDates; i < fc_leaseLiability.Count; i++)
            {
                FC_LeaseLiabilityTable leaseliabilityData = fc_leaseLiability[i];
                FC_ROUScheduleTable rouData = fc_rouSchedule[i];
                // Create and push interest and lease interest journal entries
                JEFinalTable.Add(new FC_JournalEntryTable
                {
                    JE_Date = leaseliabilityData.LeaseLiability_Date,
                    Particular = "Interest Expense",
                    Debit = (decimal)leaseliabilityData.Interest,
                    Credit = 0,
                    LeaseId = leaseSpecificData.LeaseId
                });

                JEFinalTable.Add(new FC_JournalEntryTable
                {
                    JE_Date = leaseliabilityData.LeaseLiability_Date,
                    Particular = "Lease Liability",
                    Debit = 0,
                    Credit = (decimal)leaseliabilityData.Interest,
                    LeaseId = leaseSpecificData.LeaseId
                });

                // Create and push amortization and ROU journal entries
                JEFinalTable.Add(new FC_JournalEntryTable
                {
                    JE_Date = rouData.ROU_Date,
                    Particular = "Amortization Expense",
                    Debit = (decimal)rouData.Amortization,
                    Credit = 0,
                    LeaseId = leaseSpecificData.LeaseId
                });

                JEFinalTable.Add(new FC_JournalEntryTable
                {
                    JE_Date = rouData.ROU_Date,
                    Particular = "ROU Asset",
                    Debit = 0,
                    Credit = (decimal)rouData.Amortization,
                    LeaseId = leaseSpecificData.LeaseId
                });

                // Handle payment entries if payment is greater than 0
                if (leaseliabilityData.Payment > 0)
                {
                    JEFinalTable.Add(new FC_JournalEntryTable
                    {
                        JE_Date = leaseliabilityData.LeaseLiability_Date,
                        Particular = "Lease Liability",
                        Debit = (decimal)leaseliabilityData.Payment,
                        Credit = 0,
                        LeaseId = leaseSpecificData.LeaseId
                    });

                    JEFinalTable.Add(new FC_JournalEntryTable
                    {
                        JE_Date = leaseliabilityData.LeaseLiability_Date,
                        Particular = "Bank",
                        Debit = 0,
                        Credit = (decimal)leaseliabilityData.Payment,
                        LeaseId = leaseSpecificData.LeaseId
                    });
                }
            }

            _context.FC_JournalEntries.AddRange(JEFinalTable);
            await _context.SaveChangesAsync();

            return JEFinalTable;
        }

        public async Task<JournalEntryResult> GetJEForLease(int pageNumber, int pageSize, int leaseId, DateTime? startDate, DateTime? endDate)
        {
            IEnumerable<JournalEntryTable> journalEntries = await _context.GetJournalEntriesAsync(pageNumber, pageSize, leaseId, startDate, endDate);
            int totalRecord = await _context.JournalEntries.Where(r => r.LeaseId == leaseId && (startDate == null || endDate == null || (r.JE_Date >= startDate && r.JE_Date <= endDate))).CountAsync();
            DateTime startDatet = new(2024, 3, 25);
            DateTime endDatet = new(2024, 3, 25);

            IEnumerable<JournalEntryTable> alljournalEntries = await _context.JournalEntries.Where(item => item.JE_Date >= startDatet && item.JE_Date <= endDatet).ToListAsync();

            var rouAsset = alljournalEntries.Where(item => item.Particular == "ROU Asset").Sum(item => item.Debit);
            var leaseLiability = alljournalEntries.Where(item => item.Particular == "Lease Liability").Sum(item => item.Credit);
            var leaseLiabilityDebit = alljournalEntries.Where(item => item.Particular == "Lease Liability").Sum(item => item.Debit);
            var bank = alljournalEntries.Where(item => item.Particular == "Bank").Sum(item => item.Credit);
            var interestExpense = alljournalEntries.Where(item => item.Particular == "Interest Expense").Sum(item => item.Debit);
            var amortizationExpense = alljournalEntries.Where(item => item.Particular == "Amortization Expense").Sum(item => item.Debit);
            var exchangeGainLoss = alljournalEntries.Where(item => item.Particular == "Exchange Gain/Loss").Sum(item => item.Debit);
            var exchangeGainLossCredit = alljournalEntries.Where(item => item.Particular == "Exchange Gain/Loss").Sum(item => item.Credit);

            IEnumerable<JournalEntryTable> acumilatedJE = alljournalEntries.GroupBy(item => item.Particular).Select(item => new JournalEntryTable
            {
                Particular = item.Key,
                Debit = item.Sum(item => item.Debit),
                Credit = item.Sum(item => item.Credit)
            });


            return new()
            {
                Data = journalEntries,
                TotalRecords = totalRecord,
            };
        }
    }
}
