namespace Project.Enums
{
    public enum AccStatus
    {
        Active,
        Inactive,
        Banned
    }

    public enum ProStatus
    {
        OutOfStock,
        Active,
        Pending,
        Banned
    }

    public enum OrdStatus
    {
        Pending,
        Preparing,
        OnWay,
        Recieved
    }

    public enum PersonType
    {
        Admin,
        Customer,
        Merchant,
        DeliveryRep
    }

    public enum GenderType
    {
        Male,
        Female,
       
    }

    public enum EventStatus
    {
        view,
        add_to_cart,
        add_to_wishlist,
        remove_from_cart,
        remove_from_wishlist

    }

}