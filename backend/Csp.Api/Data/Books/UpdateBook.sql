UPDATE books 
SET Title = @Title, Author = @Author, Isbn = @Isbn, Category = @Category, 
    PublishedYear = @PublishedYear, UpdatedBy = @UpdatedBy, UpdatedAt = CURRENT_TIMESTAMP
WHERE Id = @Id