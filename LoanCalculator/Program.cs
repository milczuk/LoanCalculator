using System;
using System.Runtime;
using System.CommandLine;

namespace LoanCalculator
{
    static class Program
    {
        /// <summary>
        /// Loan calculator command line program. can generate a payment overview for a simplified loan calculation given these parameter.
        /// Unit of Time could be a year, does not matter for the math to work. Just assume it's in years.
        /// Payments and capitalization by default is 12 times a year, we could have more or less, your call.
        /// just do not provide values below zero, there is no point really. I don't know if this breaks the math, I just don't want to go there. OK? 
        /// I'll let you set the amount to less then zero, let's see what happens.
        /// </summary>
        /// <param name="loanAmount">loan amount (Principal)</param>
        /// <param name="loanDuration">loan duration (in _units of time_),
        /// for simplicity we can assume it's number of years. 
        /// If you want 5.5 years long loan you'd need to mess with the engine a little 
        /// (no floats for this field allowed in v1, sorry)</param>
        /// <param name="simpleBaseUnitOfTimeInterestRate">AKA Annual Interest Rate (the fake one banks use to trick people who do not know the math behind compound interest)</param>
        /// <param name="repaymentFrequency">frequency of payments but also of applying interest in single unit of time (12 means 12 times, like monthly capitalization during a year _unit of time_.</param>
        /// <param name="maxAdmissionFee">maximum admission fee</param>
        /// <param name="admissionFeeRate"></param>
        public static void Main(
            double loanAmount = 500000,
            int loanDuration = 10,
            double simpleBaseUnitOfTimeInterestRate = 0.05,
            int repaymentFrequency = 12,
            double maxAdmissionFee = 10000,
            double admissionFeeRate = 0.01)
        {
            ValidateNumbersGreaterThan0(nameof(loanDuration), loanDuration);
            ValidateNumbersGreaterThan0(nameof(simpleBaseUnitOfTimeInterestRate), simpleBaseUnitOfTimeInterestRate);
            ValidateNumbersGreaterThan0(nameof(repaymentFrequency), repaymentFrequency);
            ValidateNumbersGreaterThan0(nameof(maxAdmissionFee), maxAdmissionFee);
            ValidateNumbersGreaterThan0(nameof(admissionFeeRate), admissionFeeRate);
            maxAdmissionFee = FixAdmissionFee(maxAdmissionFee);

            var totalCountOfRepaymentPeriods = loanDuration * repaymentFrequency;

            var monthlyPaymentAmount = CalculateConstantMonthlyPaymentAmount(
                loanAmount,
                simpleBaseUnitOfTimeInterestRate,
                repaymentFrequency,
                totalCountOfRepaymentPeriods);
            var admissionFee = CalculateAdmissionFee(loanAmount, maxAdmissionFee, admissionFeeRate);
            Console.WriteLine($"Total number of repayment periods:\t{totalCountOfRepaymentPeriods}");
            Console.WriteLine($"Monthly payment amount:\t{monthlyPaymentAmount}");
            var totalInterestRateCost = (totalCountOfRepaymentPeriods * monthlyPaymentAmount) - loanAmount;
            Console.WriteLine($"Total amount paint in interest rate for the full duration of the loan:\t{totalInterestRateCost}");
            Console.WriteLine($"Admission fee:\t{admissionFee}");

            //first guess based on interest rates alone
            var effectiveCostPerUnitOfTime = Math.Pow(1 + ((double)simpleBaseUnitOfTimeInterestRate / (double)repaymentFrequency), repaymentFrequency) - 1;
            //iterate the equsion for 20 times until minumum found
            int retryCnt = 0;
            const int MAX_RETRY_CNT = 20;

            while (retryCnt < MAX_RETRY_CNT)
            {
                delta = 
                    //all the drawdowns
                    loanAmount - admissionFee - 
                    //all the repayments

                if (Math.Abs(delta) < 0.0001)
                {
                    break;//who cares about greater precision? nobody, that's who.
                }
                retryCnt++;
            }
            Console.WriteLine($"\"ÅOP\" also known as the yearly cost as a percentage of the loan amount\t{effectiveCostPerUnitOfTime} or {effectiveCostPerUnitOfTime * 100:F2}%");
        }

        private static double FixAdmissionFee(double maxAdmissionFee)
        {
            if (maxAdmissionFee < 0)
            {
                Console.WriteLine("looks like you provided less than zero value for maximum admission fee. " +
                    "\nNo worries. " +
                    "\nI'll just assum you ment zero there. We're all good:)");
                maxAdmissionFee = 0;
            }

            return maxAdmissionFee;
        }

        private static double CalculateConstantMonthlyPaymentAmount(double loanAmount, double simpleBaseUnitOfTimeInterestRate, int paymentFrequency, int totalCountOfPaymentPeriods)
        {
            // so I just used this equasion from here: https://pl.wikipedia.org/wiki/Raty_r%C3%B3wne (sorry it's in polish...) 
            // and it worked, I mean I got the result from the example so it must be what you wanted 
            return loanAmount * simpleBaseUnitOfTimeInterestRate / (
                paymentFrequency * (1 - Math.Pow(1 / (1 + (simpleBaseUnitOfTimeInterestRate / paymentFrequency)), totalCountOfPaymentPeriods)));
        }

        private static double CalculateAdmissionFee(double loanAmount, double maxAdmissionFee, double admissionFeeRate)
            => Math.Min(maxAdmissionFee, admissionFeeRate * loanAmount);

        private static void ValidateNumbersGreaterThan0(string fieldname, IConvertible value)
        {
            if (Convert.ToDecimal(value) <= 0M)
            {
                throw new ArgumentException($"Value of {fieldname} must be grater than zero. Why U tryin to break the math?");
            }
        }
    }
}
