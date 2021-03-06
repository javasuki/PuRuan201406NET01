SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustData]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[CustData](
	[CustID] [int] IDENTITY(1,1) NOT NULL,
	[CustName] [varchar](50) NOT NULL,
 CONSTRAINT [PK_CustData] PRIMARY KEY CLUSTERED 
(
	[CustID] ASC
)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrdDetails]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[OrdDetails](
	[AutoID] [int] IDENTITY(1,1) NOT NULL,
	[OrdID] [int] NOT NULL,
	[PrdID] [int] NOT NULL,
	[Price] [money] NOT NULL,
	[QNum] [int] NOT NULL,
	[LPrice] [money] NULL,
 CONSTRAINT [PK_OrdDetails] PRIMARY KEY CLUSTERED 
(
	[AutoID] ASC
)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrdData]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[OrdData](
	[OrdID] [int] IDENTITY(1,1) NOT NULL,
	[OrdNO] [varchar](50) NOT NULL,
	[CustID] [int] NOT NULL,
	[OrdTime] [datetime] NOT NULL,
 CONSTRAINT [PK_OrdData] PRIMARY KEY CLUSTERED 
(
	[OrdID] ASC
)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PrdData]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[PrdData](
	[PrdID] [int] IDENTITY(1,1) NOT NULL,
	[PrdName] [varchar](50) NOT NULL,
	[Price] [money] NOT NULL,
 CONSTRAINT [PK_PrdData] PRIMARY KEY CLUSTERED 
(
	[PrdID] ASC
)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
