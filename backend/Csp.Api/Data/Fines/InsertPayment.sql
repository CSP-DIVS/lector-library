INSERT INTO Payments (FineId, UserId, Amount, PaymentMethod, TransactionId, Description, PaymentDate)
VALUES (@FineId, @UserId, @Amount, @PaymentMethod, @TransactionId, @Description, @PaymentDate);
SELECT LAST_INSERT_ID() AS Id;
