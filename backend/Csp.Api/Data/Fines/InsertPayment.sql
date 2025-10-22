INSERT INTO Payments (FineId, UserId, Amount, Method, TransactionId, Description, PaymentDate)
VALUES (@FineId, @UserId, @Amount, @Method, @TransactionId, @Description, @PaymentDate);
SELECT LAST_INSERT_ID() AS Id;
