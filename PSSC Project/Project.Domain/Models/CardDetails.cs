namespace Project.Domain.Models
{
    public  record CardDetails(UserCardNumber UserCardNumber, UserCardCVV UserCardCVV, UserCardExpiryDate UserCardExpiryDate, UserCardBalance UserCardBalance, bool ToUpdate);
}
