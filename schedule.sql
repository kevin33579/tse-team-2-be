SELECT  p.imageUrl     AS productImageUrl,
        pt.name        AS productTypeName,
        p.name         AS productName,
        s.time         AS scheduleTime
FROM    detail_invoice    di
JOIN    invoice           i  ON  i.id       = di.invoice_id
                              AND i.user_id = @UserId        -- user filter
JOIN    product           p  ON  p.id       = di.product_id
JOIN    producttype       pt ON  pt.id      = p.productTypeId
LEFT JOIN schedule        s  ON  s.id       = di.schedule_id
WHERE   DATE(s.time) >= CURDATE()                            -- ← today or future
ORDER BY s.time;