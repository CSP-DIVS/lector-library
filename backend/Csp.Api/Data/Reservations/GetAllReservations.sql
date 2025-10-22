SELECT r.*, b.Title, b.Author, b.Isbn, u.Username, u.Email
FROM reservations r
INNER JOIN books b ON r.BookId = b.Id
INNER JOIN users u ON r.UserId = u.Id
WHERE r.Status IN ('Pending', 'Available')
ORDER BY r.Status DESC, r.QueuePosition ASC
LIMIT @PageSize OFFSET @Offset