SELECT 
    f.Id,
    f.LendingId,
    f.UserId,
    u.Username AS MemberName,
    u.Email AS MemberEmail,
    f.BookId,
    b.Title AS BookTitle,
    b.Author AS BookAuthor,
    f.Reason,
    f.Amount,
    f.Status,
    f.DueDate,
    f.OverdueDate,
    f.DaysOverdue,
    f.CreatedAt,
    f.UpdatedAt
FROM Fines f
INNER JOIN Users u ON f.UserId = u.Id
INNER JOIN Books b ON f.BookId = b.Id
WHERE f.Id = @Id;
