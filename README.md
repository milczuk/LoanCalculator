# Loan Calculator
  ## Loan calculator command line program (interview excercise)
  This small program generates a payment overview for a simplified loan calculation given these parameter.
  Unit of Time could be a year, does not matter for the math to work. Just assume it's in years.
  Payments and capitalization by default is 12 times a year, we could have more or less, your call.
  just do not provide values below zero, there is no point really. I don't know if this breaks the math, I just don't want to go there. OK?
  I'll let you set the amount to less then zero, let's see what happens.
## Usage:
<pre>
  LoanCalculator [options]

Options:
  --loan-amount <loan-amount>                        loan amount (Principal) [default: 500000]
  --loan-duration <loan-duration>                    loan duration (in _units of time_),
                                                      for simplicity we can assume it's number of years.
                                                      If you want 5.5 years long loan you'd need to mess with the engine a little
                                                      (no floats for this field allowed in v1, sorry) [default: 10]
  --nominal-interest-rate <nominal-interest-rate>    AKA Annual Interest Rate (the fake one banks use to trick people who do not know the math behind compound interest) [default: 0,05]
  --repayment-frequency <repayment-frequency>        frequency of payments but also of applying interest in single unit of time (12 means 12 times, like monthly capitalization during a year _unit of time_. [default: 12]
  --max-admission-fee <max-admission-fee>            maximum admission fee [default: 10000]
  --admission-fee-rate <admission-fee-rate>          [default: 0,01]
  --debug-aop                                        nothing to see here, move along, just some param for testing purposes [default: False]
  --version                                          Show version information
  -?, -h, --help                                     Show help and usage information
  </pre>
