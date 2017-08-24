﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ReusableUIComponents;

namespace Dashboard.CatalogueSummary.DataQualityReporting.SubComponents
{
    /// <summary>
    /// Part of ColumnStatesChart, shows what proportion of a given column in the dataset is passing/failing validation.  See ColumnStatesChart for a description of the use case.
    /// </summary>
    [TechnicalUI]
    public partial class ConsequenceBar : UserControl
    {
        public ConsequenceBar()
        {
            InitializeComponent();
            
        }

        public static Color CorrectColor = Color.Green;
        public static Color MissingColor = Color.Orange;
        public static Color WrongColor = Color.IndianRed;
        public static Color InvalidColor = Color.Red;
        
        public static Color HasValuesColor = Color.Black;
        public static Color IsNullColor = Color.LightGray;

        public double Correct { get; set; }
        public double Invalid { get; set; }
        public double Missing { get; set; }
        public double Wrong { get; set; }
        public double DBNull { get; set; }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            
            //Control looks like this:
            //note that because null count is completely separate from consequence it has it's own microbar

            /*****************************************************************/
            //        |.............................|################|,,,,,,,,|
            //Correct |..... Missing................|### Wrong ######|,Invalid|
            //        |.............................|################|,,,,,,,,|
            //        |.............................|################|,,,,,,,,|
            ////////////////////////////////////////////////////////////////////
            //.......Nulls Rectangle............|         Not Nulls Rectangle
            /******************************************************************/

            SolidBrush bCorrect = new SolidBrush(CorrectColor);
            SolidBrush bMissing = new SolidBrush(MissingColor);
            SolidBrush bWrong = new SolidBrush(WrongColor);
            SolidBrush bInvalid = new SolidBrush(InvalidColor);

            SolidBrush bValues = new SolidBrush(HasValuesColor);
            SolidBrush bNulls = new SolidBrush(IsNullColor);

            double totalRecords = Correct + Missing + Invalid + Wrong;
            
            int heightOfNullsBarStart = (int) (Height * 0.8);
            int heightOfNullsBar = (int) (Height/5.0);
            

            //draw the nulls bar
            double valuesRatio = 1 - (DBNull / totalRecords);
            int midPointOfNullsBar = (int) (valuesRatio*Width);

            //values
            e.Graphics.FillRectangle(bValues,new Rectangle(0,heightOfNullsBarStart,midPointOfNullsBar,heightOfNullsBar));
            e.Graphics.FillRectangle(bNulls,new Rectangle(midPointOfNullsBar,heightOfNullsBarStart,Width-midPointOfNullsBar,heightOfNullsBar));
            
            
            //draw the main bar
            int correctRightPoint = (int) (((Correct)/totalRecords)*Width);

            int missingWidth = (int) ((Missing/totalRecords)*Width);
            int missingRightPoint = correctRightPoint + missingWidth;

            int wrongWidth = (int) ((Wrong/totalRecords)*Width);
            int wrongRightPoint =  missingRightPoint + wrongWidth;

            int invalidWidth = (int)((Invalid / totalRecords) * Width);
            
            e.Graphics.FillRectangle(bCorrect,new Rectangle(0,0,correctRightPoint,heightOfNullsBarStart));
            e.Graphics.FillRectangle(bMissing, new Rectangle(correctRightPoint, 0, missingWidth, heightOfNullsBarStart));
            e.Graphics.FillRectangle(bWrong, new Rectangle(missingRightPoint, 0, wrongWidth, heightOfNullsBarStart));
            e.Graphics.FillRectangle(bInvalid, new Rectangle(wrongRightPoint, 0, invalidWidth, heightOfNullsBarStart));
        }

        public void GenerateToolTip()
        {
            var toolTip = new ToolTip();

            //Lets avoid divide by zero errors
            if (Correct + Missing + Invalid + Wrong < 1)
                return;

            toolTip.SetToolTip(this,
                "Null:" +
                string.Format("{0:n0}", DBNull) + GetPercentageText(DBNull) +
                "Correct:" +
                string.Format("{0:n0}", Correct) + GetPercentageText(Correct) +
                "Missing:" +
                string.Format("{0:n0}", Missing) + GetPercentageText(Missing) +
                "Wrong:" +
                string.Format("{0:n0}", Wrong) + GetPercentageText(Wrong) +
                "Invalid:" +
                string.Format("{0:n0}", Invalid) + GetPercentageText(Invalid).TrimEnd()
                );
        }

        private string GetPercentageText(double fraction)
        {
            double totalRecords = Correct + Missing + Invalid + Wrong;
            return "(" + string.Format("{0:n2}", Truncate((fraction / totalRecords) * 100,2)) + "%" + ")" + Environment.NewLine;
        }

        private double Truncate(double value, int digits)
        {
            double mult = System.Math.Pow(10.0, digits);
            return System.Math.Truncate(value * mult) / mult;
        }
    }
}
