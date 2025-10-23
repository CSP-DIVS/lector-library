INSERT INTO Fines (LendingId, UserId, BookId, Reason, Amount, Status, DueDate, OverdueDate, DaysOverdue, CreatedAt)
VALUES (@LendingId, @UserId, @BookId, @Reason, @Amount, @Status, @DueDate, @OverdueDate, @DaysOverdue, @CreatedAt);
SELECT LAST_INSERT_ID() AS Id;
