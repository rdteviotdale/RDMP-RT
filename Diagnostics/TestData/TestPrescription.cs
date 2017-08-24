﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace Diagnostics.TestData
{
    public class TestPrescription
    {
          /// <summary>
        /// every row in data table has a weigth (the number of records in our bichemistry with this sample type, this dictionary lets you input
        /// a record number 0-maxWeight and be returned an appropriate row from the table based on its weighting
        /// </summary>
        private static Dictionary<int, int> weightToRow;
        private static int maxWeight = -1;
        private static DataTable lookupTable;

        static TestPrescription()
        {
            string toFind = typeof(TestPrescription).Namespace + ".PrescriptionCodes.csv";
            var lookup = typeof(TestPrescription).Assembly.GetManifestResourceStream(toFind);

            if (lookup == null)
                throw new Exception("Could not find embedded resource file " + toFind);
          
             
            CsvReader r = new CsvReader(new StreamReader(lookup),new CsvConfiguration(){Delimiter =","});
            
            lookupTable = new DataTable();

            r.Read();
            foreach (string header in r.FieldHeaders)
                lookupTable.Columns.Add(header);

            do
            {
                lookupTable.Rows.Add(r.CurrentRecord);
            } while (r.Read());
             
            weightToRow = new Dictionary<int, int>();

            int currentWeight = 0;
            for (int i = 0; i < lookupTable.Rows.Count; i++)
            {
                int frequency = int.Parse(lookupTable.Rows[i]["frequency"].ToString());
                
                if(frequency == 0)
                    continue;
                
                currentWeight += frequency;

                weightToRow.Add(currentWeight, i);
            }

            maxWeight = currentWeight;
        }


        public TestPrescription(Random r)
        {
            //get a random row from the lookup table - based on its representation within our biochemistry dataset
            DataRow row = GetRandomRowUsingWeight(r);

            res_seqno = row["res_seqno"].ToString();
            name = row["name"].ToString();
            formulation_code = row["formulation_code"].ToString();
            strength = row["strength"].ToString();
            measure_code = row["measure_code"].ToString();
            BNF_Code = row["BNF_Code"].ToString();
            formatted_BNF_Code = row["formatted_BNF_Code"].ToString();
            BNF_Description = row["BNF_Description"].ToString();
            Approved_Name = row["Approved_Name"].ToString();
            
            double min;
            double max;

            bool hasMin = double.TryParse(row["minQuantity"].ToString(),out min);
            bool hasMax = double.TryParse(row["maxQuantity"].ToString(),out max);

            if(hasMin && hasMax)
                Quantity = ((int)((r.NextDouble() * (max - min)) + min)).ToString();//it is a number
            else
                if(r.Next(0,2) == 0)
                    Quantity = row["minQuantity"].ToString();//it isn't a number, randomly select max or min
                else
                    Quantity = row["maxQuantity"].ToString();

        }

        private DataRow GetRandomRowUsingWeight(Random r)
        {
            int weightToGet = r.Next(maxWeight);

            //get the first key with a cumulative frequency above the one you are trying to get
            int row =  weightToRow.First(kvp => kvp.Key > weightToGet).Value;
            
            return lookupTable.Rows[row];
        }

        public string res_seqno;
        public string name;
        public string formulation_code;
        public string strength;
        public string measure_code;
        public string BNF_Code;
        public string formatted_BNF_Code;
        public string BNF_Description;
        public string Approved_Name;
        public string Quantity;
    }
}
