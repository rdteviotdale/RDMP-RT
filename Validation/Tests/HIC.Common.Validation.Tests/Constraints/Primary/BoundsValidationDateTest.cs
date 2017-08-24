﻿using System;
using System.Collections.Generic;
using HIC.Common.Validation.Constraints;
using HIC.Common.Validation.Constraints.Secondary;
using NUnit.Framework;

namespace HIC.Common.Validation.Tests.Constraints.Primary
{
    [TestFixture]
    class BoundsValidationDateTest
    {
        private Dictionary<string, object> _d;

        [SetUp]
        public void SetUp()
        {
            _d = new Dictionary<string, object> {{"somedate", new DateTime(2013, 06, 13)}};
        }

        #region Literal Dates

        [Test]
        public void must_occur_between_two_literal_dates_VALID()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);
            b.Lower = DateTime.MinValue;
            b.Upper = DateTime.MaxValue;
            var v = CreateLiteralDateValidator(b);

            Assert.IsNull(v.Validate(_d));
        }

        [Test]
        public void must_occur_between_two_literal_dates_INVALID_after()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);
            b.Lower = DateTime.MinValue;
            b.Upper = DateTime.MinValue.AddYears(1);
            var v = CreateLiteralDateValidator(b);

            Assert.NotNull(v.Validate(_d));
        }

        [Test]
        public void must_occur_between_two_literal_dates_INVALID_before()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);
            b.Lower = DateTime.MaxValue.AddYears(-1);
            b.Upper = DateTime.MaxValue;
            var v = CreateLiteralDateValidator(b);

            Assert.NotNull(v.Validate(_d));
        }

        [Test]
        public void must_occur_between_two_literal_dates_INVALID_onlower()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);
            b.Lower = new DateTime(2013, 06, 13);
            b.Upper = DateTime.MaxValue;
            b.Inclusive = false;

            var v = CreateLiteralDateValidator(b);

            Assert.NotNull(v.Validate(_d));
        }

        [Test]
        public void must_occur_between_two_literal_dates_INVALID_onupper()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);
            b.Lower = DateTime.MinValue;
            b.Upper = new DateTime(2013, 06, 13);
            b.Inclusive = false;
            var v = CreateLiteralDateValidator(b);

            Assert.NotNull(v.Validate(_d));
        }

        [Test]
        public void must_occur_inclusively_between_two_literal_dates_VALID_onlower()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);
            b.Lower = new DateTime(2013, 06, 13);
            b.Upper = DateTime.MaxValue;
            b.Inclusive = true;
            var v = CreateLiteralDateValidator(b);

            Assert.IsNull(v.Validate(_d));
        }

        [Test]
        public void must_occur_inclusively_between_two_literal_dates_VALID_onupper()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);
            b.Lower = DateTime.MinValue;
            b.Upper = new DateTime(2013, 06, 13);
            b.Inclusive = true;
            var v = CreateLiteralDateValidator(b);

            Assert.IsNull(v.Validate(_d));
        }

        #endregion

        #region Other Date Fields

        [Test]
        public void must_occur_after_field_VALID()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);
            b.LowerFieldName = "dob";
            b.Upper = DateTime.MaxValue;

            var v = CreateAdmissionDateValidator(b);

            Assert.IsNull(v.Validate(TestConstants.AdmissionDateOccursAfterDob));
        }


        [Test]
        public void must_occur_after_field_INVALID_before()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);
            b.LowerFieldName = "dob";
            b.Upper = DateTime.MaxValue;

            var v = CreateAdmissionDateValidator(b);

            Assert.NotNull(v.Validate(TestConstants.AdmissionDateOccursBeforeDob));
        }

        [Test]
        public void must_occur_after_field_INVALID_same()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);
            b.LowerFieldName = "dob";
            b.Upper = DateTime.MaxValue;
            b.Inclusive = false;
            var v = CreateAdmissionDateValidator(b);

            Assert.NotNull(v.Validate(TestConstants.AdmissionDateOccursOnDob));
        }

        [Test]
        public void must_occur_after_field_INVALID_violation_report()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);
            b.LowerFieldName = "dob";
            b.Upper = DateTime.MaxValue;

            var v = CreateAdmissionDateValidator(b);

            ValidationFailure result = v.Validate(TestConstants.AdmissionDateOccursBeforeDob);
            
            if(result == null)
            Assert.Fail();
            
            List<ValidationFailure> l = result.GetExceptionList();

            StringAssert.EndsWith("Expected a date greater than [" + b.LowerFieldName +"].", l[0].Message);
            Console.WriteLine(result.Message);
            
        }

        [Test]
        public void must_occur_before_field_VALID()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);
            b.Lower = DateTime.MinValue;
            b.UpperFieldName = "dob";

            var v = CreateParentDobValidator(b);

            Assert.IsNull(v.Validate(TestConstants.ParentDobOccursBeforeDob));
        }

        [Test]
        public void must_occur_before_field_INVALID_same()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);
            b.Lower = DateTime.MinValue;
            b.UpperFieldName = "dob";

            var v = CreateParentDobValidator(b);

            Assert.NotNull(v.Validate(TestConstants.ParentDobOccursOnDob));
        }

        [Test]
        public void must_occur_before_field_INVALID_after()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);
            b.Lower = DateTime.MinValue;
            b.UpperFieldName = "dob";

            var v = CreateParentDobValidator(b);

            Assert.NotNull(v.Validate(TestConstants.ParentDobOccursAfterDob));
        }

        [Test]
        public void must_occur_before_field_INVALID_violation_report()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);
            b.Lower = DateTime.MinValue;
            b.UpperFieldName = "dob";

            var v = CreateParentDobValidator(b);

            ValidationFailure result = v.Validate(TestConstants.ParentDobOccursAfterDob); 
            
            if(result == null)
                Assert.Fail();

            List<ValidationFailure> l = result.GetExceptionList();

            StringAssert.EndsWith("Expected a date less than [" + b.UpperFieldName + "].", l[0].Message);
            Console.WriteLine(result.Message);
            
        }

        [Test]
        public void must_occur_between_fields_VALID()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);
            b.LowerFieldName = "admission_date";
            b.UpperFieldName = "discharge_date";

            var v = CreateOperationDateValidator(b);

            Assert.IsNull(v.Validate(TestConstants.OperationOccursDuringStay));
        }

        [Test]
        public void must_occur_inclusively_between_fields_VALID_onstart()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);
            b.LowerFieldName = "admission_date";
            b.UpperFieldName = "discharge_date";
            b.Inclusive = true;

            var v = CreateOperationDateValidator(b);

            Assert.IsNull(v.Validate(TestConstants.OperationOccursOnStartOfStay));
        }

        [Test]
        public void must_occur_inclusively_between_fields_VALID_onend()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);
            b.LowerFieldName = "admission_date";
            b.UpperFieldName = "discharge_date";
            b.Inclusive = true;

            var v = CreateOperationDateValidator(b);

            Assert.IsNull(v.Validate(TestConstants.OperationOccursOnEndOfStay));
        }

        [Test]
        public void must_occur_between_fields_INVALID_onstart()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);
            b.LowerFieldName = "admission_date";
            b.UpperFieldName = "discharge_date";
            b.Inclusive = false;
            var v = CreateOperationDateValidator(b);

            Assert.NotNull(v.Validate(TestConstants.OperationOccursOnStartOfStay));
        }

        [Test]
        public void must_occur_between_fields_INVALID_onend()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);
            b.LowerFieldName = "admission_date";
            b.UpperFieldName = "discharge_date";
            b.Inclusive = false;
            var v = CreateOperationDateValidator(b);

            Assert.NotNull(v.Validate(TestConstants.OperationOccursOnEndOfStay));
        }

        [Test]
        public void must_occur_between_fields_INVALID_before()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);
            b.LowerFieldName = "admission_date";
            b.UpperFieldName = "discharge_date";

            var v = CreateOperationDateValidator(b);

            Assert.NotNull(v.Validate(TestConstants.OperationOccursBeforeStay));
        }

        [Test]
        public void must_occur_between_fields_INVALID_after()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);
            b.LowerFieldName = "admission_date";
            b.UpperFieldName = "discharge_date";

            var v = CreateOperationDateValidator(b);

            Assert.NotNull(v.Validate(TestConstants.OperationOccursAfterStay));
        }

        #endregion

        #region Fluent API experiment

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void f_invalid_target_field_evokes_exception()
        {
            var v = new Validator();
            v.EnsureThatValue("INVALID").OccursAfter("dob");

            v.Validate(TestConstants.AdmissionDateOccursAfterDob);
        }

        [Test]
        [Ignore("Thomas, can you fix please?")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void f_invalid_comparator_field_evokes_exception()
        {
            var v = new Validator();
            v.EnsureThatValue("admission_date").OccursAfter("INVALID");

            v.Validate(TestConstants.AdmissionDateOccursAfterDob);
        }

        [Test]
        public void f_must_occur_after_field_VALID()
        {
            var v = new Validator();
            v.EnsureThatValue("admission_date").OccursAfter("dob");

            Assert.IsNull(v.Validate(TestConstants.AdmissionDateOccursAfterDob));
        }

        [Test]
        public void f_must_occur_after_field_INVALID_before()
        {
            var v = new Validator();
            v.EnsureThatValue("admission_date").OccursAfter("dob");

            Assert.NotNull(v.Validate(TestConstants.AdmissionDateOccursBeforeDob));
        }

        [Test]
        public void f_must_occur_after_field_INVALID_same()
        {
            var v = new Validator();
            v.EnsureThatValue("admission_date").OccursAfter("dob");

            Assert.NotNull(v.Validate(TestConstants.AdmissionDateOccursOnDob));
        }

        [Test]
        public void f_must_occur_before_field_VALID()
        {
            var v = new Validator();
            v.EnsureThatValue("parent_dob").OccursBefore("dob");

            Assert.IsNull(v.Validate(TestConstants.ParentDobOccursBeforeDob));
        }

        [Test]
        public void f_must_occur_before_field_INVALID_after()
        {
            var v = new Validator();
            v.EnsureThatValue("parent_dob").OccursBefore("dob");

            Assert.NotNull(v.Validate(TestConstants.ParentDobOccursAfterDob));
        }

        [Test]
        public void f_must_occur_before_field_INVALID_same()
        {
            var v = new Validator();
            v.EnsureThatValue("parent_dob").OccursBefore("dob");
            
            Assert.NotNull(v.Validate(TestConstants.ParentDobOccursOnDob));
        }

        #endregion

        #region Helper Methods
        private static Validator CreateLiteralDateValidator(BoundDate b)
        {
            var v = new Validator();
            var i = new ItemValidator();
            i.AddSecondaryConstraint(b);
            v.AddItemValidator(i, "somedate", typeof(DateTime));

            return v;
        }

        private static Validator CreateAdmissionDateValidator(BoundDate b)
        {
            var v = new Validator();
            var i = new ItemValidator();
            i.AddSecondaryConstraint(b);
            v.AddItemValidator(i, "admission_date", typeof(DateTime));
            
            return v;
        }

        private static Validator CreateParentDobValidator(BoundDate b)
        {
            var v = new Validator();
            var i = new ItemValidator();
            i.AddSecondaryConstraint(b);
            v.AddItemValidator(i, "parent_dob", typeof(DateTime));

            return v;
        }

        private static Validator CreateOperationDateValidator(BoundDate b)
        {
            var v = new Validator();
            var i = new ItemValidator();
            i.AddSecondaryConstraint(b);
            v.AddItemValidator(i, "operation_date", typeof(DateTime));

            return v;
        }

        #endregion
    }
}





