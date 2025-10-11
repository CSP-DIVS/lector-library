UPDATE lendings
SET FineAmount = @NewAmount,
    FinePaid = @FinePaid,
    UpdatedAt = @UpdatedAt
WHERE Id = @LendingId;
