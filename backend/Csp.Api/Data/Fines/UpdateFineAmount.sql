UPDATE Fines 
SET Amount = @NewAmount, UpdatedAt = @UpdatedAt 
WHERE Id = @Id;
