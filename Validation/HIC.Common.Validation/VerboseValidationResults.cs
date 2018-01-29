﻿using System;
using System.Collections.Generic;
using System.Linq;
using HIC.Common.Validation.Constraints;

namespace HIC.Common.Validation
{
    /// <summary>
    /// Storage class for recording the number of rows failing validation with each Consequence subdivided by Column.
    /// </summary>
    public class VerboseValidationResults
    {
        /// <summary>
        /// Dictionary of column names (Key), Value is a Dictionary of each of the potential consequences
        /// and a count of the number of cells that failed validation with that Consequence (for that Column - Key)
        /// 
        /// e.g. DictionaryOfFailure["Forename"][Consequence.Missing] is a count of the number of cells which are missing
        /// (not there where they were expected) in column Forename
        /// 
        /// </summary>
        public Dictionary<string, Dictionary<Consequence, int>> DictionaryOfFailure { get; private set; }

        /// <summary>
        /// Every time a row is Invalidated (this List get's the reason for Invalidation added to it)
        /// </summary>
        public List<string> ReasonsRowsInvalidated { get; set; }

        /// <summary>
        /// A count of the rows Invalidated due to dodgy data - failed Validations with Consequence.InvalidatesRow
        /// </summary>
        public int CountOfRowsInvalidated {get; set; }

        public VerboseValidationResults(ItemValidator[] validators)
        {
            CountOfRowsInvalidated = 0;
            ReasonsRowsInvalidated = new List<string>();
            DictionaryOfFailure = new Dictionary<string, Dictionary<Consequence, int>>();

            foreach (ItemValidator iv in validators)
            {
                DictionaryOfFailure.Add(iv.TargetProperty,null);
                DictionaryOfFailure[iv.TargetProperty] = new Dictionary<Consequence, int>();
                DictionaryOfFailure[iv.TargetProperty].Add(Consequence.Missing,0);
                DictionaryOfFailure[iv.TargetProperty].Add(Consequence.Wrong, 0);
                DictionaryOfFailure[iv.TargetProperty].Add(Consequence.InvalidatesRow, 0);
            }
        }


        public Consequence ProcessException(ValidationFailure rootValidationFailure)
        {
            try
            {
                ConfirmIntegrityOfValidationException(rootValidationFailure);

                Dictionary<ItemValidator, Consequence> worstConsequences = new Dictionary<ItemValidator, Consequence>();

                foreach (var subException in rootValidationFailure.GetExceptionList())
                {
                    if (!subException.SourceConstraint.Consequence.HasValue)
                        throw new NullReferenceException("ItemValidator of type " + subException.SourceItemValidator.GetType().Name + " on column " + subException.SourceItemValidator.TargetProperty + " has not had it's Consequence configured");

                    //we have encountered a rule that will invalidate the entire row, it's a good idea to keep a track of each of these since it would be rubbish to get a report out the other side that simply says 100% of rows invalid!
                    if (subException.SourceConstraint.Consequence == Consequence.InvalidatesRow)
                        if (!ReasonsRowsInvalidated.Contains(subException.SourceItemValidator.TargetProperty + "|" + subException.SourceConstraint.GetType().Name))
                            ReasonsRowsInvalidated.Add(subException.SourceItemValidator.TargetProperty + "|" + subException.SourceConstraint.GetType().Name);

                    if (worstConsequences.Keys.Contains(subException.SourceItemValidator) == true)
                    {
                        //see if situation got worse
                        Consequence oldConsequence = worstConsequences[subException.SourceItemValidator];
                        Consequence newConsequence = subException.SourceConstraint.Consequence.Value;

                        if (newConsequence > oldConsequence)
                            worstConsequences[subException.SourceItemValidator] = newConsequence;
                    }
                    else
                    {
                        //new validation error for this column
                        worstConsequences.Add(subException.SourceItemValidator, (Consequence)subException.SourceConstraint.Consequence);
                    }
                }

                //now record the worst case event
                if (worstConsequences.Values.Contains(Consequence.InvalidatesRow))
                    CountOfRowsInvalidated++;
                
                foreach (var itemValidator in worstConsequences.Keys)
                {
                    string columnName = itemValidator.TargetProperty;
                    
                    //increment the most damaging consequence count for this cell
                    DictionaryOfFailure[columnName][worstConsequences[itemValidator]]++;
                }

                return worstConsequences.Max(key => key.Value);
            }
            catch (Exception e)
            {
                throw new Exception("An error occurred when trying to process a ValidationException into Verbose results:" + e.Message, e);
            }
        }

        private void ConfirmIntegrityOfValidationException(ValidationFailure v)
        {
            if (v.GetExceptionList() == null)
                throw new NullReferenceException("Expected ValidationException to produce a List of broken validations, not just 1.  Validation message was:" + v.Message);

            foreach (var validationException in v.GetExceptionList())
            {
                if (validationException.SourceItemValidator == null || validationException.SourceItemValidator.TargetProperty == null)
                    throw new NullReferenceException("Column name referenced in ValidationException was null!, message in the exception was:" + validationException.Message);

                if (validationException.GetExceptionList() != null)
                    throw new Exception("Inception ValidationException detected! not only does the root Exception have a list of subexceptions (expected) but one of those has a sublist too! (unexpected).  This Exception message was:" + validationException.Message);
            }
        }
    }
}
