using iri.core.conf;

namespace iri.core
{
    public class  Iota
    {
        public TransactionValidator TransactionValidator { get; }

        public Iota(Configuration configuration)
        {

            TransactionValidator = new TransactionValidator();
            //TransactionValidator = new TransactionValidator(tangle, tipsViewModel, transactionRequester, messageQ);
        }

        public void Init()
        {
            //TODO(gjc): add code here
         
        }
    }
}
