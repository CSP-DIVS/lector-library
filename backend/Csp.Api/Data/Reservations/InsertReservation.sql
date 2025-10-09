INSERT INTO reservations (BookId, UserId, ReservedDate, QueuePosition, Status)
VALUES (@BookId, @UserId, @ReservedDate, @QueuePosition, 'Pending');
SELECT LAST_INSERT_ID();