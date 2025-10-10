INSERT INTO lendings (BookId, UserId, BorrowDate, DueDate, Status, RenewalCount, MaxRenewals)
VALUES (@BookId, @UserId, @BorrowDate, @DueDate, 'Active', 0, 2);
SELECT LAST_INSERT_ID();