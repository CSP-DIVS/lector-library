INSERT INTO books (Title, Author, Isbn, Category, PublishedYear, IsActive, CreatedBy, UpdatedBy)
VALUES (@Title, @Author, @Isbn, @Category, @PublishedYear, 1, @CreatedBy, @UpdatedBy);
SELECT LAST_INSERT_ID();