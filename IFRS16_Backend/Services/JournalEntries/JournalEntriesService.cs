using EFCore.BulkExtensions;
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

        public async Task<List<JournalEntryTable>> PostJEForLease(LeaseFormData leaseSpecificData, List<LeaseLiabilityTable> leaseLiability, List<ROUScheduleTable> rouSchedule, ModificationDetails? modificationDetails = null, bool fromRemeasure = false)
        {
            int startTableDates = 0;
            LeaseLiabilityTable leaseMustField = leaseLiability[0];
            ROUScheduleTable respectiveROU = rouSchedule[0];
            List<JournalEntryTable> JEFinalTable = [];

            if (!fromRemeasure && modificationDetails == null)
            {
                JEFinalTable.Add(new JournalEntryTable
                {
                    JE_Date = leaseMustField.LeaseLiability_Date,
                    Particular = "21025010 - Lease Liability",
                    Debit = 0,
                    Credit = (decimal)(leaseMustField.Opening - leaseMustField.Payment),
                    LeaseId = leaseSpecificData.LeaseId
                });
                JEFinalTable.Add(new JournalEntryTable
                {
                    JE_Date = respectiveROU.ROU_Date,
                    Particular = "11510060 - Right of Use Asset",
                    Debit = (decimal)(respectiveROU.Opening - (leaseSpecificData.IDC ?? 0)),
                    Credit = 0,
                    LeaseId = leaseSpecificData.LeaseId
                });
                if ((leaseMustField.Opening - (respectiveROU.Opening - (leaseSpecificData.IDC ?? 0)) != 0))
                {
                    decimal PNL = (decimal)(leaseMustField.Opening - (respectiveROU.Opening - (leaseSpecificData.IDC ?? 0)));
                    JEFinalTable.Add(new JournalEntryTable
                    {
                        JE_Date = respectiveROU.ROU_Date,
                        Particular = "Profit and loss",
                        Debit = PNL > 0 ? PNL : 0,
                        Credit = PNL < 0 ? PNL : 0,
                        LeaseId = leaseSpecificData.LeaseId
                    });
                }

            }
            if (modificationDetails != null)
            {
                if (modificationDetails.ModificationLoss != 0)
                {
                    JEFinalTable.Add(new JournalEntryTable
                    {
                        JE_Date = leaseMustField.LeaseLiability_Date,
                        Particular = "21025010 - Lease Liability",
                        Debit = (decimal)modificationDetails.LeaseLiability,
                        Credit = 0,
                        LeaseId = leaseSpecificData.LeaseId
                    });
                    JEFinalTable.Add(new JournalEntryTable
                    {
                        JE_Date = respectiveROU.ROU_Date,
                        Particular = "11510060 - Right of Use Asset",
                        Debit = 0,
                        Credit = (decimal)modificationDetails.Rou,
                        LeaseId = leaseSpecificData.LeaseId
                    });
                    JEFinalTable.Add(new JournalEntryTable
                    {
                        JE_Date = respectiveROU.ROU_Date,
                        Particular = "Modification loss",
                        Debit = modificationDetails.ModificationLoss > 0 ? (decimal)modificationDetails.ModificationLoss : 0,
                        Credit = modificationDetails.ModificationLoss < 0 ? (decimal)modificationDetails.ModificationLoss : 0,
                        LeaseId = leaseSpecificData.LeaseId
                    });
                }
                if (modificationDetails.ModificationLoss == 0)
                {
                    JEFinalTable.Add(new JournalEntryTable
                    {
                        JE_Date = leaseMustField.LeaseLiability_Date,
                        Particular = "21025010 - Lease Liability",
                        Debit = modificationDetails.ModificationAdjustment < 0 ? (decimal)modificationDetails.ModificationAdjustment : 0,
                        Credit = modificationDetails.ModificationAdjustment > 0 ? (decimal)modificationDetails.ModificationAdjustment : 0,
                        LeaseId = leaseSpecificData.LeaseId
                    });
                    JEFinalTable.Add(new JournalEntryTable
                    {
                        JE_Date = respectiveROU.ROU_Date,
                        Particular = "11510060 - Right of Use Asset",
                        Debit = modificationDetails.ModificationAdjustment > 0 ? (decimal)modificationDetails.ModificationAdjustment : 0,
                        Credit = modificationDetails.ModificationAdjustment < 0 ? (decimal)modificationDetails.ModificationAdjustment : 0,
                        LeaseId = leaseSpecificData.LeaseId
                    });
                }


            }

            // Handle payment
            if (leaseMustField.Payment > 0)
            {
                JEFinalTable.Add(new JournalEntryTable
                {
                    JE_Date = leaseMustField.LeaseLiability_Date,
                    Particular = "21010012 - Payable",
                    Debit = 0,
                    Credit = (decimal)leaseMustField.Payment,
                    LeaseId = leaseSpecificData.LeaseId
                });
            }

            if (modificationDetails == null)
            {
                // Handle IDC
                if (leaseSpecificData.IDC.HasValue && leaseSpecificData.IDC != 0)
                {
                    JEFinalTable.Add(new JournalEntryTable
                    {
                        JE_Date = respectiveROU.ROU_Date,
                        Particular = "11510060 - Right of Use Asset",
                        Debit = (decimal)leaseSpecificData.IDC,
                        Credit = 0,
                        LeaseId = leaseSpecificData.LeaseId
                    });

                    JEFinalTable.Add(new JournalEntryTable
                    {
                        JE_Date = respectiveROU.ROU_Date,
                        Particular = "Payable (Initial Direct Cost)",
                        Debit = 0,
                        Credit = (decimal)leaseSpecificData.IDC,
                        LeaseId = leaseSpecificData.LeaseId
                    });
                }

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
                    Particular = "71510005 - Interest Expense",
                    Debit = (decimal)leaseliabilityData.Interest,
                    Credit = 0,
                    LeaseId = leaseSpecificData.LeaseId
                });

                JEFinalTable.Add(new JournalEntryTable
                {
                    JE_Date = leaseliabilityData.LeaseLiability_Date,
                    Particular = "21025010 - Lease Liability",
                    Debit = 0,
                    Credit = (decimal)leaseliabilityData.Interest,
                    LeaseId = leaseSpecificData.LeaseId
                });

                if (exhangeGainLossChecking != 0 && leaseliabilityData?.Exchange_Gain_Loss != null)
                {
                    JEFinalTable.Add(new JournalEntryTable
                    {
                        JE_Date = leaseliabilityData.LeaseLiability_Date,
                        Particular = "72010010 - Exchange Gain/Loss",
                        Debit = (decimal)leaseliabilityData?.Exchange_Gain_Loss < 0 ? Math.Abs((decimal)leaseliabilityData.Exchange_Gain_Loss) : 0,
                        Credit = (decimal)leaseliabilityData?.Exchange_Gain_Loss > 0 ? (decimal)leaseliabilityData.Exchange_Gain_Loss : 0,
                        LeaseId = leaseSpecificData.LeaseId
                    });
                    JEFinalTable.Add(new JournalEntryTable
                    {
                        JE_Date = leaseliabilityData.LeaseLiability_Date,
                        Particular = "21025010 - Lease Liability",
                        Debit = (decimal)leaseliabilityData?.Exchange_Gain_Loss > 0 ? (decimal)leaseliabilityData.Exchange_Gain_Loss : 0,
                        Credit = (decimal)leaseliabilityData?.Exchange_Gain_Loss < 0 ? Math.Abs((decimal)leaseliabilityData.Exchange_Gain_Loss) : 0,
                        LeaseId = leaseSpecificData.LeaseId
                    });
                }

                // Create and push amortization and ROU journal entries
                JEFinalTable.Add(new JournalEntryTable
                {
                    JE_Date = rouData.ROU_Date,
                    Particular = "71010010 - Amortization Expense",
                    Debit = (decimal)rouData.Amortization,
                    Credit = 0,
                    LeaseId = leaseSpecificData.LeaseId
                });

                JEFinalTable.Add(new JournalEntryTable
                {
                    JE_Date = rouData.ROU_Date,
                    Particular = "11510060 - Right of Use Asset",
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
                        Particular = "21025010 - Lease Liability",
                        Debit = (decimal)leaseliabilityData.Payment,
                        Credit = 0,
                        LeaseId = leaseSpecificData.LeaseId
                    });

                    JEFinalTable.Add(new JournalEntryTable
                    {
                        JE_Date = leaseliabilityData.LeaseLiability_Date,
                        Particular = "21010012 - Payable",
                        Debit = 0,
                        Credit = (decimal)leaseliabilityData.Payment,
                        LeaseId = leaseSpecificData.LeaseId
                    });
                }
            }
            await _context.BulkInsertAsync(JEFinalTable);

            return JEFinalTable;
        }

        public async Task<List<FC_JournalEntryTable>> PostJEForLeaseforFC(LeaseFormData leaseSpecificData, List<FC_LeaseLiabilityTable> fc_leaseLiability, List<FC_ROUScheduleTable> fc_rouSchedule)
        {
            int startTableDates = leaseSpecificData.Annuity == "advance" ? 1 : 0;
            FC_LeaseLiabilityTable leaseMustField = fc_leaseLiability[0];
            FC_ROUScheduleTable respectiveROU = fc_rouSchedule[0];
            List<FC_JournalEntryTable> JEFinalTable = new List<FC_JournalEntryTable>();

            JEFinalTable.Add(new FC_JournalEntryTable
            {
                JE_Date = respectiveROU.ROU_Date,
                Particular = "11510060 - Right of Use Asset",
                Debit = (decimal)(respectiveROU.Opening - (leaseSpecificData.IDC ?? 0)),
                Credit = 0,
                LeaseId = leaseSpecificData.LeaseId
            });
            JEFinalTable.Add(new FC_JournalEntryTable
            {
                JE_Date = leaseMustField.LeaseLiability_Date,
                Particular = "21025010 - Lease Liability",
                Debit = 0,
                Credit = (decimal)(leaseMustField.Opening - leaseMustField.Payment),
                LeaseId = leaseSpecificData.LeaseId
            });

            // Handle payment
            if (leaseMustField.Payment > 0)
            {
                JEFinalTable.Add(new FC_JournalEntryTable
                {
                    JE_Date = leaseMustField.LeaseLiability_Date,
                    Particular = "21010012 - Payable",
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
                    Particular = "11510060 - Right of Use Asset",
                    Debit = (decimal)leaseSpecificData.IDC,
                    Credit = 0,
                    LeaseId = leaseSpecificData.LeaseId
                });

                JEFinalTable.Add(new FC_JournalEntryTable
                {
                    JE_Date = respectiveROU.ROU_Date,
                    Particular = "Payable (IDC)",
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
                    Particular = "71510005 - Interest Expense",
                    Debit = (decimal)leaseliabilityData.Interest,
                    Credit = 0,
                    LeaseId = leaseSpecificData.LeaseId
                });

                JEFinalTable.Add(new FC_JournalEntryTable
                {
                    JE_Date = leaseliabilityData.LeaseLiability_Date,
                    Particular = "21025010 - Lease Liability",
                    Debit = 0,
                    Credit = (decimal)leaseliabilityData.Interest,
                    LeaseId = leaseSpecificData.LeaseId
                });

                // Create and push amortization and ROU journal entries
                JEFinalTable.Add(new FC_JournalEntryTable
                {
                    JE_Date = rouData.ROU_Date,
                    Particular = "71010010 - Amortization Expense",
                    Debit = (decimal)rouData.Amortization,
                    Credit = 0,
                    LeaseId = leaseSpecificData.LeaseId
                });

                JEFinalTable.Add(new FC_JournalEntryTable
                {
                    JE_Date = rouData.ROU_Date,
                    Particular = "11510060 - Right of Use Asset",
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
                        Particular = "21025010 - Lease Liability",
                        Debit = (decimal)leaseliabilityData.Payment,
                        Credit = 0,
                        LeaseId = leaseSpecificData.LeaseId
                    });

                    JEFinalTable.Add(new FC_JournalEntryTable
                    {
                        JE_Date = leaseliabilityData.LeaseLiability_Date,
                        Particular = "21010012 - Payable",
                        Debit = 0,
                        Credit = (decimal)leaseliabilityData.Payment,
                        LeaseId = leaseSpecificData.LeaseId
                    });
                }
            }

            await _context.BulkInsertAsync(JEFinalTable);

            return JEFinalTable;
        }

        public async Task<JournalEntryResult> GetJEForLease(int pageNumber, int pageSize, int leaseId, DateTime? startDate, DateTime? endDate)
        {
            IEnumerable<JournalEntryTable> journalEntries = await _context.GetJournalEntriesAsync(pageNumber, pageSize, leaseId, startDate, endDate);
            int totalRecord = await _context.JournalEntries.Where(r => r.LeaseId == leaseId && (startDate == null || endDate == null || (r.JE_Date >= startDate && r.JE_Date <= endDate))).CountAsync();
            return new()
            {
                Data = journalEntries,
                TotalRecords = totalRecord,
            };
        }
        public async Task<List<JournalEntryTable>> GetAllJEForLease(int leaseId)
        {
            List<JournalEntryTable> journalEntries = await _context.JournalEntries.Where(r => r.LeaseId == leaseId).ToListAsync();
            return journalEntries;
        }

        public async Task<IEnumerable<JournalEntryTable>> EnterJEOnTermination(decimal LLClosing, decimal ROUClosing, decimal? Penalty, DateTime terminationDate, int leaseId)
        {
            List<JournalEntryTable> JEFinalTable = new List<JournalEntryTable>();

            JEFinalTable.Add(new JournalEntryTable
            {
                JE_Date = terminationDate,
                Particular = "11510060 - Right of Use Asset",
                Debit = 0,
                Credit = ROUClosing,
                LeaseId = leaseId
            });
            JEFinalTable.Add(new JournalEntryTable
            {
                JE_Date = terminationDate,
                Particular = "21025010 - Lease Liability",
                Debit = LLClosing,
                Credit = 0,
                LeaseId = leaseId
            });
            JEFinalTable.Add(new JournalEntryTable
            {
                JE_Date = terminationDate,
                Particular = "Penalty Payable",
                Debit = 0,
                Credit = Penalty ?? 0,
                LeaseId = leaseId
            });

            decimal terminationGainLoss = LLClosing - (ROUClosing + Penalty ?? 0);
            //terminationGainLoss if + ve then credit and - ve then debit
            JEFinalTable.Add(new JournalEntryTable
            {
                JE_Date = terminationDate,
                Particular = "Termination Gain/Loss",
                Debit = terminationGainLoss < 0 ? Math.Abs(terminationGainLoss) : 0,
                Credit = terminationGainLoss > 0 ? terminationGainLoss : 0,
                LeaseId = leaseId
            });

            _context.JournalEntries.AddRange(JEFinalTable);
            await _context.SaveChangesAsync();

            return JEFinalTable;
        }
    }
}
