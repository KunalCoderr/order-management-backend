USE [OrderManagement]
GO
ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [FK__Orders__UserId__2B3F6F97]
GO
ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [FK__Orders__ProductI__2C3393D0]
GO
ALTER TABLE [dbo].[Users] DROP CONSTRAINT [DF__Users__CreatedAt__25869641]
GO
ALTER TABLE [dbo].[Products] DROP CONSTRAINT [DF__Products__Create__286302EC]
GO
ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [DF__Orders__OrderDat__2D27B809]
GO
/****** Object:  Index [UQ__Users__536C85E4F7574C20]    Script Date: 8/28/2025 3:53:33 PM ******/
ALTER TABLE [dbo].[Users] DROP CONSTRAINT [UQ__Users__536C85E4F7574C20]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 8/28/2025 3:53:33 PM ******/
DROP TABLE [dbo].[Users]
GO
/****** Object:  Table [dbo].[Products]    Script Date: 8/28/2025 3:53:33 PM ******/
DROP TABLE [dbo].[Products]
GO
/****** Object:  Table [dbo].[Orders]    Script Date: 8/28/2025 3:53:33 PM ******/
DROP TABLE [dbo].[Orders]
GO
/****** Object:  Table [dbo].[Orders]    Script Date: 8/28/2025 3:53:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Orders](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NULL,
	[ProductId] [int] NULL,
	[Quantity] [int] NOT NULL,
	[OrderDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Products]    Script Date: 8/28/2025 3:53:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Products](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 8/28/2025 3:53:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Username] [nvarchar](100) NOT NULL,
	[PasswordHash] [nvarchar](255) NOT NULL,
	[PasswordSalt] [nvarchar](255) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Orders] ON 
GO
INSERT [dbo].[Orders] ([Id], [UserId], [ProductId], [Quantity], [OrderDate]) VALUES (1, 1, 5, 2, CAST(N'2025-08-26T16:29:41.707' AS DateTime))
GO
INSERT [dbo].[Orders] ([Id], [UserId], [ProductId], [Quantity], [OrderDate]) VALUES (2, 1, 3, 2, CAST(N'2025-08-26T17:05:09.583' AS DateTime))
GO
INSERT [dbo].[Orders] ([Id], [UserId], [ProductId], [Quantity], [OrderDate]) VALUES (3, 1, 6, 1, CAST(N'2025-08-26T17:05:09.807' AS DateTime))
GO
INSERT [dbo].[Orders] ([Id], [UserId], [ProductId], [Quantity], [OrderDate]) VALUES (4, 1, 3, 1, CAST(N'2025-08-26T17:15:16.573' AS DateTime))
GO
INSERT [dbo].[Orders] ([Id], [UserId], [ProductId], [Quantity], [OrderDate]) VALUES (5, 1, 6, 4, CAST(N'2025-08-26T17:19:48.393' AS DateTime))
GO
INSERT [dbo].[Orders] ([Id], [UserId], [ProductId], [Quantity], [OrderDate]) VALUES (6, 1, 6, 4, CAST(N'2025-08-26T17:23:39.763' AS DateTime))
GO
INSERT [dbo].[Orders] ([Id], [UserId], [ProductId], [Quantity], [OrderDate]) VALUES (7, 1, 3, 2, CAST(N'2025-08-28T12:18:49.207' AS DateTime))
GO
INSERT [dbo].[Orders] ([Id], [UserId], [ProductId], [Quantity], [OrderDate]) VALUES (8, 1, 6, 1, CAST(N'2025-08-28T12:18:49.237' AS DateTime))
GO
INSERT [dbo].[Orders] ([Id], [UserId], [ProductId], [Quantity], [OrderDate]) VALUES (9, 1, 3, 2, CAST(N'2025-08-28T15:44:21.410' AS DateTime))
GO
INSERT [dbo].[Orders] ([Id], [UserId], [ProductId], [Quantity], [OrderDate]) VALUES (10, 1, 6, 1, CAST(N'2025-08-28T15:44:21.450' AS DateTime))
GO
INSERT [dbo].[Orders] ([Id], [UserId], [ProductId], [Quantity], [OrderDate]) VALUES (11, 1, 3, 9, CAST(N'2025-08-28T15:44:44.957' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[Orders] OFF
GO
SET IDENTITY_INSERT [dbo].[Products] ON 
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CreatedAt]) VALUES (3, N'iphone 14', N'Testing update', CAST(14000.00 AS Decimal(18, 2)), CAST(N'2025-08-22T13:22:49.960' AS DateTime))
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CreatedAt]) VALUES (5, N'Samsung S25 Ultra', N'Samsung s25 ultra ', CAST(35000.00 AS Decimal(18, 2)), CAST(N'2025-08-25T13:10:11.020' AS DateTime))
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CreatedAt]) VALUES (6, N'Samsung Ultra', N'testing again', CAST(40000.00 AS Decimal(18, 2)), CAST(N'2025-08-28T10:13:34.100' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[Products] OFF
GO
SET IDENTITY_INSERT [dbo].[Users] ON 
GO
INSERT [dbo].[Users] ([Id], [Username], [PasswordHash], [PasswordSalt], [CreatedAt]) VALUES (1, N'Kunal Kumar', N'g/gNMfB/r7ot/iM9lcfLf0SGHGDFJ93iLk4cXlbPe+s=', N'fjMa3YPDPHf7WfcXp0RaWlUd+5DlOQAKbY9lJdNQm+CXHocKMdMUSSql3v885vaJTCbDLqpoikT86tMcKpiyMA==', CAST(N'2025-08-22T07:21:35.020' AS DateTime))
GO
INSERT [dbo].[Users] ([Id], [Username], [PasswordHash], [PasswordSalt], [CreatedAt]) VALUES (2, N'Test 1', N'WSnWw4JLPfP93VE+khPMx247MrgUX1ne5qFXDwAbEz0=', N'8bQsa+ekbKX3Q7BWoAudVF64iH6U9tDaGrHaH0WOLcN8xZaF/OapMb0ekmJ1THeWvru2T0B05vFCVFZvw/3wtw==', CAST(N'2025-08-22T07:25:42.013' AS DateTime))
GO
INSERT [dbo].[Users] ([Id], [Username], [PasswordHash], [PasswordSalt], [CreatedAt]) VALUES (3, N'Mohan', N'WLvDvL0ySrXaTjQGV31qIYGCsIrwQ3h6sHjP4WJ8Knw=', N'XyIn5FApQj+koELTo0UNMa5fjveTm+cYKJAIUIMoAzwXLj8cjRFcDW/znlYeo5tqZkX8x5DeisE1UNx9UNrGoA==', CAST(N'2025-08-28T06:50:53.523' AS DateTime))
GO
INSERT [dbo].[Users] ([Id], [Username], [PasswordHash], [PasswordSalt], [CreatedAt]) VALUES (4, N'Mohan 1', N'5ODW4/Dccm1CUMLRt/6T5q95vf6n2X736dtsUepXffc=', N'blCGD+saCiQJaOtwtf21xeqQ3ie9/atWaEsjqyXaIgeI/0zLoY61qBhiqQTm3hujtkXYpXTuS/5DV8iwYf2S0A==', CAST(N'2025-08-28T06:53:17.400' AS DateTime))
GO
INSERT [dbo].[Users] ([Id], [Username], [PasswordHash], [PasswordSalt], [CreatedAt]) VALUES (5, N'Mohan 2', N'xyXITrZ1Q0s6S/A4E55QzCxB88ah8mbu1aQL0aTe8uI=', N'NjDIougOStqsibTALTwaZA+91bKNll/8fxO4D2I96jlvL+UIjkCwvpUueCU6gSkBdv2iwRe5Eu8NH72p8ImadQ==', CAST(N'2025-08-28T06:53:51.963' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[Users] OFF
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Users__536C85E4F7574C20]    Script Date: 8/28/2025 3:53:34 PM ******/
ALTER TABLE [dbo].[Users] ADD UNIQUE NONCLUSTERED 
(
	[Username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Orders] ADD  DEFAULT (getutcdate()) FOR [OrderDate]
GO
ALTER TABLE [dbo].[Products] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
GO
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO
