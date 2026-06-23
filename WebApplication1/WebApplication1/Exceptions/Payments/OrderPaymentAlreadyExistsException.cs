namespace WebApplication1.Exceptions.Payments;

public class OrderPaymentAlreadyExistsException(int orderId)
    : ConflictException($"Order '{orderId}' already has a payment.");
