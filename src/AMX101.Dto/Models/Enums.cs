namespace AMX101.Dto.Models
{
    public class Consts
    {
        public const string Claims = "claims";
        public const string StaticClaims = "staticClaims";
        public const string Sources = "sources";
        public const string PostCodes = "postCodes";
        public const string ClaimValues = "claimValues";
    }


    public enum Industry
    {
        All = 0,
        Retail = 1,
        Dining = 2,
        Lodge = 3
    }

    public enum Category
    {
        General = 0,
        Retail = 1,
        Online = 2,
        Lodge = 3,
        Dining = 4
    }

    public enum Type
    {
        Static = 0,
        DynamicPrimary = 1,
        DynamicSecondary = 2
    }

    /// <summary>
    /// ClaimType. Reflects Claim.[Id,Heading]
    /// </summary>
    public enum ClaimType
    {
        /// <summary>
        /// Cards in force = 1
        /// </summary>
        Cardsinforce = 1,
        /// <summary>
        /// Transactions = 2
        /// </summary>
        Transactions = 2,
        /// <summary>
        /// Retail Transactions = 3
        /// </summary>
        RetailTransactions = 3,
        /// <summary>
        /// Dining Transactions = 4
        /// </summary>
        DiningTransactions = 4,
        /// <summary>
        /// Lodge Transactions = 5
        /// </summary>
        LodgeTransactions = 5,
        /// <summary>
        /// Spent at Retail Merchants = 6
        /// </summary>
        Spentatmerchants = 6,
        /// <summary>
        /// Spent at Retail Merchants = 7
        /// </summary>
        Spentatretailmerchants = 7,
        /// <summary>
        /// Spent at Dining Merchants = 8
        /// </summary>
        Spentatdiningmerchants,
        /// <summary>
        /// Spent at Lodge Merchants = 9
        /// </summary>
        Spentatlodgemerchants
    }
}
