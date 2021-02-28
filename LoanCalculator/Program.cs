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
        /// <param name="nominalInterestRate">AKA Annual Interest Rate (the fake one banks use to trick people who do not know the math behind compound interest)</param>
        /// <param name="repaymentFrequency">frequency of payments but also of applying interest in single unit of time (12 means 12 times, like monthly capitalization during a year _unit of time_.</param>
        /// <param name="maxAdmissionFee">maximum admission fee</param>
        /// <param name="admissionFeeRate"></param>
        /// <param name="debugÅOP">nothing to see here, move along, just some param for testing purposes</param>
        public static void Main(
            double loanAmount = 500000,
            int loanDuration = 10,
            double nominalInterestRate = 0.05,
            int repaymentFrequency = 12,
            double maxAdmissionFee = 10000,
            double admissionFeeRate = 0.01,
            bool debugÅOP = false)
        {
            ValidateNumbersGreaterThan0(nameof(loanDuration), loanDuration);
            ValidateNumbersGreaterThan0(nameof(repaymentFrequency), repaymentFrequency);
            ValidateNumbersGreaterThan0(nameof(nominalInterestRate), nominalInterestRate);
            maxAdmissionFee = FixIfLessThanZero(nameof(maxAdmissionFee), maxAdmissionFee);
            admissionFeeRate = FixIfLessThanZero(nameof(admissionFeeRate), admissionFeeRate);

            var totalCountOfRepaymentPeriods = loanDuration * repaymentFrequency;

            var monthlyPaymentAmount =
                CalculateConstantMonthlyPaymentAmount(
                    loanAmount,
                    nominalInterestRate,
                    repaymentFrequency,
                    totalCountOfRepaymentPeriods);

            var admissionFee = CalculateAdmissionFee(loanAmount, maxAdmissionFee, admissionFeeRate);
            var totalInterestRateCost = (totalCountOfRepaymentPeriods * monthlyPaymentAmount) - loanAmount;

            Console.WriteLine($"Total number of repayment periods:\t{totalCountOfRepaymentPeriods}");
            Console.WriteLine($"Monthly payment amount:\t{monthlyPaymentAmount}");
            Console.WriteLine($"Total amount paint in interest rate for the full duration of the loan:\t{totalInterestRateCost}");
            Console.WriteLine($"Admission fee:\t{admissionFee}");

            double effectiveCostPerUnitOfTime = CalculateÅOPLikeExplainedOnWikipedia(
                loanAmount,
                nominalInterestRate,
                repaymentFrequency,
                totalCountOfRepaymentPeriods,
                monthlyPaymentAmount,
                admissionFee,
                debugÅOP);
            Console.WriteLine(
                "\"ÅOP\" also known as the yearly cost as a percentage of " +
                $"the loan amount\t{effectiveCostPerUnitOfTime} ~= {effectiveCostPerUnitOfTime * 100:F3}%");
        }

        private static double CalculateÅOPLikeExplainedOnWikipedia(
            double loanAmount,
            double nominalInterestRate,
            int repaymentFrequency,
            int totalCountOfRepaymentPeriods,
            double monthlyPaymentAmount,
            double admissionFee,
            bool debugÅOP)
        {
            //first guess based on interest rates alone
            var effectiveCostPerUnitOfTime = Math.Pow(1 + (nominalInterestRate / repaymentFrequency), repaymentFrequency) - 1;
            //iterate the equsion for 20 times until minumum found
            int retryCnt = 0;
            const int MAX_RETRY_CNT = 200;

            // poor man's golden split search interpolation
            var step = 0.5d;
            while (retryCnt < MAX_RETRY_CNT)
            {
                var sum = 0d;
                for (var i = 1; i <= totalCountOfRepaymentPeriods; i++)
                {
                    sum += monthlyPaymentAmount / Math.Pow(1 + effectiveCostPerUnitOfTime, i / repaymentFrequency);
                }
                var delta =
                    //(all the drawdowns - all the fees (including admission fee) - all repayments) ---> we want this to be equal to 0 (zero)
                    loanAmount - admissionFee - sum;

                if (debugÅOP)
                {
                    Console.WriteLine($"{effectiveCostPerUnitOfTime:F5} delta {retryCnt,2} = {delta,15:F5}");
                }

                if (Math.Abs(delta) < 0.0001)
                {
                    break; // precise enough
                }

                var direction = delta < 0 ? 1 : -1;
                effectiveCostPerUnitOfTime += step * direction;
                if (effectiveCostPerUnitOfTime < 0)
                    effectiveCostPerUnitOfTime = 0;// it get's weird when we go below zero, so let's avoid that

                step *= 0.666;
                retryCnt++;
            }

            return effectiveCostPerUnitOfTime;
        }

        private static double FixIfLessThanZero(string fieldName, double maxAdmissionFee)
        {
            if (maxAdmissionFee < 0)
            {
                Console.WriteLine($"looks like you provided less than zero value for {fieldName}. " +
                    "\nNo worries. " +
                    "\nI'll just assum you ment zero there. We're all good:)");
                maxAdmissionFee = 0;
            }

            return maxAdmissionFee;
        }

        private static double CalculateConstantMonthlyPaymentAmount(double loanAmount, double nominalInterestRate, int repaymentFrequency, int totalCountOfPaymentPeriods)
        {
            // so I just used this equasion from here: https://pl.wikipedia.org/wiki/Raty_r%C3%B3wne (sorry it's in polish...) 
            // and it worked, I mean I got the result from the example so it must be what you wanted 
            return loanAmount * nominalInterestRate / (
                repaymentFrequency * (1 - Math.Pow(1 / (1 + (nominalInterestRate / repaymentFrequency)), totalCountOfPaymentPeriods)));
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
