IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [categories] (
    [id] int NOT NULL IDENTITY,
    [name] nvarchar(max) NOT NULL,
    [created_at] datetime2 NOT NULL,
    CONSTRAINT [pk_categories] PRIMARY KEY ([id])
);

CREATE TABLE [users] (
    [id] int NOT NULL IDENTITY,
    [first_name] nvarchar(max) NOT NULL,
    [last_name] nvarchar(max) NOT NULL,
    [email] nvarchar(450) NOT NULL,
    [password_hash] nvarchar(max) NOT NULL,
    [phone] nvarchar(max) NULL,
    [profile_image] nvarchar(max) NULL,
    [role] nvarchar(max) NOT NULL,
    [created_at] datetime2 NOT NULL,
    [updated_at] datetime2 NULL,
    CONSTRAINT [pk_users] PRIMARY KEY ([id]),
    CONSTRAINT [ck_users_role] CHECK (role IN ('customer', 'admin'))
);

CREATE TABLE [products] (
    [id] int NOT NULL IDENTITY,
    [category_id] int NOT NULL,
    [name] nvarchar(max) NOT NULL,
    [description] nvarchar(max) NOT NULL,
    [price] decimal(18,2) NOT NULL,
    [discount_price] decimal(18,2) NULL,
    [stock_quantity] int NOT NULL,
    [material] nvarchar(max) NULL,
    [color] nvarchar(max) NULL,
    [height] decimal(18,2) NULL,
    [width] decimal(18,2) NULL,
    [depth] decimal(18,2) NULL,
    [is_active] bit NOT NULL,
    [is_featured] bit NOT NULL,
    [average_rating] decimal(3,2) NOT NULL,
    [reviews_count] int NOT NULL,
    [created_at] datetime2 NOT NULL,
    [updated_at] datetime2 NULL,
    CONSTRAINT [pk_products] PRIMARY KEY ([id]),
    CONSTRAINT [fk_products_categories_category_id] FOREIGN KEY ([category_id]) REFERENCES [categories] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [addresses] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NOT NULL,
    [country] nvarchar(max) NOT NULL,
    [city] nvarchar(max) NOT NULL,
    [street] nvarchar(max) NOT NULL,
    [building] nvarchar(max) NULL,
    [postal_code] nvarchar(max) NULL,
    [is_default] bit NOT NULL,
    [created_at] datetime2 NOT NULL,
    CONSTRAINT [pk_addresses] PRIMARY KEY ([id]),
    CONSTRAINT [fk_addresses__users_user_id] FOREIGN KEY ([user_id]) REFERENCES [users] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [ai_chats] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NOT NULL,
    [title] nvarchar(max) NOT NULL,
    [created_at] datetime2 NOT NULL,
    [updated_at] datetime2 NULL,
    CONSTRAINT [pk_ai_chats] PRIMARY KEY ([id]),
    CONSTRAINT [fk_ai_chats__users_user_id] FOREIGN KEY ([user_id]) REFERENCES [users] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [cart] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NOT NULL,
    [created_at] datetime2 NOT NULL,
    [updated_at] datetime2 NULL,
    CONSTRAINT [pk_carts] PRIMARY KEY ([id]),
    CONSTRAINT [fk_carts__users_user_id] FOREIGN KEY ([user_id]) REFERENCES [users] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [room_uploads] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NOT NULL,
    [image_url] nvarchar(max) NOT NULL,
    [room_type] nvarchar(max) NOT NULL,
    [height] decimal(18,2) NOT NULL,
    [width] decimal(18,2) NOT NULL,
    [depth] decimal(18,2) NOT NULL,
    [created_at] datetime2 NOT NULL,
    CONSTRAINT [pk_room_uploads] PRIMARY KEY ([id]),
    CONSTRAINT [fk_room_uploads__users_user_id] FOREIGN KEY ([user_id]) REFERENCES [users] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [favorites] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NOT NULL,
    [product_id] int NOT NULL,
    [created_at] datetime2 NOT NULL,
    CONSTRAINT [pk_favorites] PRIMARY KEY ([id]),
    CONSTRAINT [fk_favorites__products_product_id] FOREIGN KEY ([product_id]) REFERENCES [products] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_favorites__users_user_id] FOREIGN KEY ([user_id]) REFERENCES [users] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [product_images] (
    [id] int NOT NULL IDENTITY,
    [product_id] int NOT NULL,
    [image_url] nvarchar(max) NOT NULL,
    [is_main] bit NOT NULL,
    [created_at] datetime2 NOT NULL,
    CONSTRAINT [pk_product_images] PRIMARY KEY ([id]),
    CONSTRAINT [fk_product_images_products_product_id] FOREIGN KEY ([product_id]) REFERENCES [products] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [product_variants] (
    [id] int NOT NULL IDENTITY,
    [product_id] int NOT NULL,
    [color] nvarchar(max) NULL,
    [size] nvarchar(max) NULL,
    [material] nvarchar(max) NULL,
    [sku] nvarchar(450) NOT NULL,
    [price] decimal(18,2) NOT NULL,
    [stock_quantity] int NOT NULL,
    [created_at] datetime2 NOT NULL,
    CONSTRAINT [pk_product_variants] PRIMARY KEY ([id]),
    CONSTRAINT [fk_product_variants_products_product_id] FOREIGN KEY ([product_id]) REFERENCES [products] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [reviews] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NOT NULL,
    [product_id] int NOT NULL,
    [rating] int NOT NULL,
    [comment] nvarchar(max) NULL,
    [created_at] datetime2 NOT NULL,
    CONSTRAINT [pk_reviews] PRIMARY KEY ([id]),
    CONSTRAINT [ck_reviews_rating] CHECK (rating BETWEEN 1 AND 5),
    CONSTRAINT [fk_reviews__users_user_id] FOREIGN KEY ([user_id]) REFERENCES [users] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_reviews_products_product_id] FOREIGN KEY ([product_id]) REFERENCES [products] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [orders] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NOT NULL,
    [address_id] int NOT NULL,
    [total_price] decimal(18,2) NOT NULL,
    [status] nvarchar(max) NOT NULL,
    [payment_status] nvarchar(max) NOT NULL,
    [stripe_payment_intent_id] nvarchar(max) NULL,
    [created_at] datetime2 NOT NULL,
    [updated_at] datetime2 NULL,
    CONSTRAINT [pk_orders] PRIMARY KEY ([id]),
    CONSTRAINT [ck_orders_payment_status] CHECK (payment_status IN ('pending', 'paid', 'failed')),
    CONSTRAINT [ck_orders_status] CHECK (status IN ('pending', 'processing', 'shipped', 'delivered', 'cancelled')),
    CONSTRAINT [fk_orders__users_user_id] FOREIGN KEY ([user_id]) REFERENCES [users] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_orders_addresses_address_id] FOREIGN KEY ([address_id]) REFERENCES [addresses] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [ai_messages] (
    [id] int NOT NULL IDENTITY,
    [chat_id] int NOT NULL,
    [role] nvarchar(max) NOT NULL,
    [content] nvarchar(max) NOT NULL,
    [created_at] datetime2 NOT NULL,
    CONSTRAINT [pk_ai_messages] PRIMARY KEY ([id]),
    CONSTRAINT [fk_ai_messages_ai_chats_ai_chat_id] FOREIGN KEY ([chat_id]) REFERENCES [ai_chats] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [room_designs] (
    [id] int NOT NULL IDENTITY,
    [room_upload_id] int NOT NULL,
    [name] nvarchar(max) NOT NULL,
    [created_at] datetime2 NOT NULL,
    [updated_at] datetime2 NULL,
    CONSTRAINT [pk_room_designs] PRIMARY KEY ([id]),
    CONSTRAINT [fk_room_designs__room_uploads_room_upload_id] FOREIGN KEY ([room_upload_id]) REFERENCES [room_uploads] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [cart_items] (
    [id] int NOT NULL IDENTITY,
    [cart_id] int NOT NULL,
    [product_id] int NOT NULL,
    [variant_id] int NULL,
    [quantity] int NOT NULL,
    [created_at] datetime2 NOT NULL,
    CONSTRAINT [pk_cart_items] PRIMARY KEY ([id]),
    CONSTRAINT [fk_cart_items__product_variants_product_variant_id] FOREIGN KEY ([variant_id]) REFERENCES [product_variants] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_cart_items__products_product_id] FOREIGN KEY ([product_id]) REFERENCES [products] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_cart_items_carts_cart_id] FOREIGN KEY ([cart_id]) REFERENCES [cart] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [order_items] (
    [id] int NOT NULL IDENTITY,
    [order_id] int NOT NULL,
    [product_id] int NOT NULL,
    [variant_id] int NULL,
    [quantity] int NOT NULL,
    [price] decimal(18,2) NOT NULL,
    [created_at] datetime2 NOT NULL,
    CONSTRAINT [pk_order_items] PRIMARY KEY ([id]),
    CONSTRAINT [fk_order_items__product_variants_product_variant_id] FOREIGN KEY ([variant_id]) REFERENCES [product_variants] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_order_items__products_product_id] FOREIGN KEY ([product_id]) REFERENCES [products] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_order_items_orders_order_id] FOREIGN KEY ([order_id]) REFERENCES [orders] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [payments] (
    [id] int NOT NULL IDENTITY,
    [order_id] int NOT NULL,
    [stripe_payment_intent_id] nvarchar(max) NULL,
    [stripe_charge_id] nvarchar(max) NULL,
    [amount] decimal(18,2) NOT NULL,
    [currency] nvarchar(max) NOT NULL,
    [status] nvarchar(max) NOT NULL,
    [created_at] datetime2 NOT NULL,
    CONSTRAINT [pk_payments] PRIMARY KEY ([id]),
    CONSTRAINT [ck_payments_status] CHECK (status IN ('pending', 'succeeded', 'failed')),
    CONSTRAINT [fk_payments_orders_order_id] FOREIGN KEY ([order_id]) REFERENCES [orders] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [room_furniture_placements] (
    [id] int NOT NULL IDENTITY,
    [room_design_id] int NOT NULL,
    [product_id] int NOT NULL,
    [position_x] decimal(18,2) NOT NULL,
    [position_y] decimal(18,2) NOT NULL,
    [rotation] decimal(18,2) NOT NULL,
    [scale] decimal(18,2) NOT NULL,
    [created_at] datetime2 NOT NULL,
    CONSTRAINT [pk_room_furniture_placements] PRIMARY KEY ([id]),
    CONSTRAINT [fk_room_furniture_placements_products_product_id] FOREIGN KEY ([product_id]) REFERENCES [products] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_room_furniture_placements_room_designs_room_design_id] FOREIGN KEY ([room_design_id]) REFERENCES [room_designs] ([id]) ON DELETE NO ACTION
);

CREATE INDEX [ix_addresses_user_id] ON [addresses] ([user_id]);

CREATE INDEX [ix_ai_chats_user_id] ON [ai_chats] ([user_id]);

CREATE INDEX [ix_ai_messages_ai_chat_id] ON [ai_messages] ([chat_id]);

CREATE UNIQUE INDEX [ix_carts_user_id] ON [cart] ([user_id]);

CREATE INDEX [ix_cart_items_cart_id] ON [cart_items] ([cart_id]);

CREATE INDEX [ix_cart_items_product_id] ON [cart_items] ([product_id]);

CREATE INDEX [ix_cart_items_product_variant_id] ON [cart_items] ([variant_id]);

CREATE INDEX [ix_favorites_product_id] ON [favorites] ([product_id]);

CREATE INDEX [ix_favorites_user_id] ON [favorites] ([user_id]);

CREATE INDEX [ix_order_items_order_id] ON [order_items] ([order_id]);

CREATE INDEX [ix_order_items_product_id] ON [order_items] ([product_id]);

CREATE INDEX [ix_order_items_product_variant_id] ON [order_items] ([variant_id]);

CREATE INDEX [ix_orders_address_id] ON [orders] ([address_id]);

CREATE INDEX [ix_orders_user_id] ON [orders] ([user_id]);

CREATE UNIQUE INDEX [ix_payments_order_id] ON [payments] ([order_id]);

CREATE INDEX [ix_product_images_product_id] ON [product_images] ([product_id]);

CREATE INDEX [ix_product_variants_product_id] ON [product_variants] ([product_id]);

CREATE UNIQUE INDEX [ix_product_variants_sku] ON [product_variants] ([sku]);

CREATE INDEX [ix_products_category_id] ON [products] ([category_id]);

CREATE INDEX [ix_reviews_product_id] ON [reviews] ([product_id]);

CREATE INDEX [ix_reviews_user_id] ON [reviews] ([user_id]);

CREATE INDEX [ix_room_designs_room_upload_id] ON [room_designs] ([room_upload_id]);

CREATE INDEX [ix_room_furniture_placements_product_id] ON [room_furniture_placements] ([product_id]);

CREATE INDEX [ix_room_furniture_placements_room_design_id] ON [room_furniture_placements] ([room_design_id]);

CREATE INDEX [ix_room_uploads_user_id] ON [room_uploads] ([user_id]);

CREATE UNIQUE INDEX [ix_users_email] ON [users] ([email]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260603002024_InitialCreate', N'10.0.8');

COMMIT;
GO

