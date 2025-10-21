SELECT 
    l.Id AS LendingId,
    l.UserId,
    l.BookId,
    l.DueDate,
    DATEDIFF(UTC_TIMESTAMP(), l.DueDate) AS DaysOverdue,
    (DATEDIFF(UTC_TIMESTAMP(), l.DueDate) * 20.0) AS FineAmount
FROM Lendings l
LEFT JOIN Fines f ON l.Id = f.LendingId
WHERE l.ReturnDate IS NULL
AND f.Id IS NULL
AND l.DueDate < UTC_TIMESTAMP()
AND DATEDIFF(UTC_TIMESTAMP(), l.DueDate) > 0;
