USE [master]
GO
/****** Object:  Database [DataExportManager2]    Script Date: 11/02/2015 09:28:19 ******/
CREATE DATABASE [DataExportManager2]
GO
ALTER DATABASE [DataExportManager2] SET COMPATIBILITY_LEVEL = 110
GO
USE [DataExportManager2]

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [DataExportManager2].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [DataExportManager2] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [DataExportManager2] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [DataExportManager2] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [DataExportManager2] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [DataExportManager2] SET ARITHABORT OFF 
GO
ALTER DATABASE [DataExportManager2] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [DataExportManager2] SET AUTO_CREATE_STATISTICS ON 
GO
ALTER DATABASE [DataExportManager2] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [DataExportManager2] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [DataExportManager2] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [DataExportManager2] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [DataExportManager2] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [DataExportManager2] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [DataExportManager2] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [DataExportManager2] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [DataExportManager2] SET  DISABLE_BROKER 
GO
ALTER DATABASE [DataExportManager2] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [DataExportManager2] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [DataExportManager2] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [DataExportManager2] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [DataExportManager2] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [DataExportManager2] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [DataExportManager2] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [DataExportManager2] SET RECOVERY FULL 
GO
ALTER DATABASE [DataExportManager2] SET  MULTI_USER 
GO
ALTER DATABASE [DataExportManager2] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [DataExportManager2] SET DB_CHAINING OFF 
GO
ALTER DATABASE [DataExportManager2] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [DataExportManager2] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
EXEC sys.sp_db_vardecimal_storage_format N'DataExportManager2', N'ON'
GO
USE [DataExportManager2]
GO
/****** Object:  User [DATAENTRY\sxdonaldsonbuist]    Script Date: 11/02/2015 09:28:19 ******/
CREATE USER [DATAENTRY\sxdonaldsonbuist] FOR LOGIN [DATAENTRY\sxdonaldsonbuist] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  User [DATAENTRY\dpaul]    Script Date: 11/02/2015 09:28:20 ******/
CREATE USER [DATAENTRY\dpaul] FOR LOGIN [DATAENTRY\dpaul] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_datareader] ADD MEMBER [DATAENTRY\sxdonaldsonbuist]
GO
ALTER ROLE [db_datawriter] ADD MEMBER [DATAENTRY\sxdonaldsonbuist]
GO
ALTER ROLE [db_datareader] ADD MEMBER [DATAENTRY\dpaul]
GO
ALTER ROLE [db_datawriter] ADD MEMBER [DATAENTRY\dpaul]
GO
/****** Object:  Table [dbo].[CohortCustomColumn]    Script Date: 11/02/2015 09:28:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CohortCustomColumn](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Cohort_ID] [int] NOT NULL,
	[SelectSQL] [varchar](500) NULL,
 CONSTRAINT [PK_CohortCustomColumn] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[CumulativeExtractionResults]    Script Date: 11/02/2015 09:28:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CumulativeExtractionResults](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ExtractionConfiguration_ID] [int] NOT NULL,
	[ExtractableDataSet_ID] [int] NOT NULL,
	[DateOfExtraction] [datetime] NOT NULL,
	[Filename] [varchar](max) NULL,
	[RecordsExtracted] [int] NOT NULL,
	[DistinctReleaseIdentifiersEncountered] [int] NOT NULL,
	[FiltersUsed] [varchar](max) NULL,
	[Exception] [varchar](max) NULL,
	[SQLExecuted] [varchar](max) NULL,
	[CohortExtracted] [int] NULL,
 CONSTRAINT [PK_CumulativeExtractionResults] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[DataUser]    Script Date: 11/02/2015 09:28:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DataUser](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Forename] [varchar](50) NOT NULL,
	[Surname] [varchar](50) NOT NULL,
	[Email] [varchar](100) NULL,
 CONSTRAINT [PK_DataUser] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[DeployedExtractionFilter]    Script Date: 11/02/2015 09:28:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DeployedExtractionFilter](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[WhereSQL] [varchar](max) NULL,
	[Description] [varchar](500) NULL,
	[Name] [varchar](100) NOT NULL,
	[FilterContainer_ID] [int] NULL,
	[IsMandatory] [bit] NOT NULL,
 CONSTRAINT [PK_ExtractionFilter] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[DeployedExtractionFilterParameter]    Script Date: 11/02/2015 09:28:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DeployedExtractionFilterParameter](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ExtractionFilter_ID] [int] NOT NULL,
	[ParameterSQL] [varchar](500) NULL,
	[Value] [varchar](500) NULL,
	[Comment] [varchar](500) NULL,
 CONSTRAINT [PK_ExtractionFilterParameter] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ExtractableCohort]    Script Date: 11/02/2015 09:28:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ExtractableCohort](
	[ID] [int] NOT NULL,
	[ReleaseIdentifierSQL] [varchar](500) NULL,
	[PrivateIdentifierSQL] [varchar](500) NULL,
 CONSTRAINT [PK_Cohort] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ExtractableColumn]    Script Date: 11/02/2015 09:28:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ExtractableColumn](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ExtractableDataSet_ID] [int] NOT NULL,
	[SelectSQL] [varchar](500) NULL,
	[Order] [int] NULL,
	[Alias] [varchar](100) NULL,
	[ExtractionConfiguration_ID] [int] NOT NULL,
	[CatalogueExtractionInformation_ID] [int] NULL,
	[HashOnDataRelease] [bit] NULL,
	[IsExtractionIdentifier] [bit] NOT NULL,
	[IsPrimaryKey] [bit] NOT NULL,
 CONSTRAINT [PK_ExtractableColumn] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ExtractableDataSet]    Script Date: 11/02/2015 09:28:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExtractableDataSet](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Catalogue_ID] [int] NULL,
	[DisableExtraction] [bit] NOT NULL,
 CONSTRAINT [PK_ExtractableDataSet] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ExtractionConfiguration]    Script Date: 11/02/2015 09:28:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ExtractionConfiguration](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[dtCreated] [datetime] NULL,
	[Project_ID] [int] NULL,
	[Username] [varchar](50) NULL,
	[Cohort_ID] [int] NULL,
	[JIRARequestTicket] [varchar](10) NULL,
	[JIRAReleaseTicket] [varchar](10) NULL,
	[Separator] [varchar](3) NOT NULL,
	[Description] [varchar](5000) NULL,
	[IsReleased] [bit] NOT NULL,
	[ClonedFrom_ID] [int] NULL,
 CONSTRAINT [PK_ExtractionConfiguration] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[FilterContainer]    Script Date: 11/02/2015 09:28:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[FilterContainer](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Operation] [varchar](10) NULL,
 CONSTRAINT [PK_FilterContainer] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[FilterContainerSubcontainers]    Script Date: 11/02/2015 09:28:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FilterContainerSubcontainers](
	[FilterContainer_ParentID] [int] NOT NULL,
	[FilterContainerChildID] [int] NOT NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[GlobalExtractionFilterParameter]    Script Date: 11/02/2015 09:28:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[GlobalExtractionFilterParameter](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ExtractionConfiguration_ID] [int] NULL,
	[Value] [varchar](500) NULL,
	[ParameterSQL] [varchar](500) NULL,
	[Comment] [varchar](500) NULL,
 CONSTRAINT [PK_GlobalExtractionFilterParameter] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Project]    Script Date: 11/02/2015 09:28:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Project](
	[Name] [varchar](300) NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[JIRAMasterTicket] [varchar](10) NULL,
	[ExtractionDirectory] [varchar](300) NULL,
	[ProjectNumber] [int] NULL,
 CONSTRAINT [PK_Project] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Project_DataUser]    Script Date: 11/02/2015 09:28:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Project_DataUser](
	[Project_ID] [int] NOT NULL,
	[DataUser_ID] [int] NOT NULL,
 CONSTRAINT [PK_Project_DataUser] PRIMARY KEY CLUSTERED 
(
	[Project_ID] ASC,
	[DataUser_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SelectedDataSets]    Script Date: 11/02/2015 09:28:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SelectedDataSets](
	[ExtractionConfiguration_ID] [int] NOT NULL,
	[ExtractableDataSet_ID] [int] NOT NULL,
	[RootFilterContainer_ID] [int] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SelectedSupplementalDatasets]    Script Date: 11/02/2015 09:28:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SelectedSupplementalDatasets](
	[Catalogue_ID] [int] NOT NULL,
	[ExtractionConfiguration_ID] [int] NOT NULL,
	[ExtractionSQL] [varchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Index [DatasetConfigurationIdx_Unique]    Script Date: 11/02/2015 09:28:20 ******/
CREATE UNIQUE NONCLUSTERED INDEX [DatasetConfigurationIdx_Unique] ON [dbo].[CumulativeExtractionResults]
(
	[ExtractionConfiguration_ID] ASC,
	[ExtractableDataSet_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [PreventDoubleAddingCatalogueIdx]    Script Date: 11/02/2015 09:28:20 ******/
CREATE UNIQUE NONCLUSTERED INDEX [PreventDoubleAddingCatalogueIdx] ON [dbo].[ExtractableDataSet]
(
	[Catalogue_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [PreventDuplicateParameterNamesInExtractionConfiguration_idx]    Script Date: 11/02/2015 09:28:20 ******/
CREATE UNIQUE NONCLUSTERED INDEX [PreventDuplicateParameterNamesInExtractionConfiguration_idx] ON [dbo].[GlobalExtractionFilterParameter]
(
	[ExtractionConfiguration_ID] ASC,
	[ParameterSQL] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CumulativeExtractionResults] ADD  CONSTRAINT [DF_CumulativeExtractionResults_DateOfExtraction]  DEFAULT (getdate()) FOR [DateOfExtraction]
GO
ALTER TABLE [dbo].[CumulativeExtractionResults] ADD  CONSTRAINT [DF_CumulativeExtractionResults_RecordsExtracted]  DEFAULT ((0)) FOR [RecordsExtracted]
GO
ALTER TABLE [dbo].[CumulativeExtractionResults] ADD  CONSTRAINT [DF_CumulativeExtractionResults_DistinctReleaseIdentifiersEncountered]  DEFAULT ((0)) FOR [DistinctReleaseIdentifiersEncountered]
GO
ALTER TABLE [dbo].[DeployedExtractionFilter] ADD  CONSTRAINT [DF_DeployedExtractionFilter_IsMandatory]  DEFAULT ((0)) FOR [IsMandatory]
GO
ALTER TABLE [dbo].[ExtractableCohort] ADD  CONSTRAINT [DF_Cohort_ReleaseIdentifierSQL]  DEFAULT ('PROCHI') FOR [ReleaseIdentifierSQL]
GO
ALTER TABLE [dbo].[ExtractableColumn] ADD  CONSTRAINT [DF_ExtractableColumn_HashOnDataRelease]  DEFAULT ((0)) FOR [HashOnDataRelease]
GO
ALTER TABLE [dbo].[ExtractableColumn] ADD  CONSTRAINT [DF_ExtractableColumn_IsExtractionIdentifier]  DEFAULT ((0)) FOR [IsExtractionIdentifier]
GO
ALTER TABLE [dbo].[ExtractableColumn] ADD  CONSTRAINT [DF_ExtractableColumn_IsPrimaryKey]  DEFAULT ((0)) FOR [IsPrimaryKey]
GO
ALTER TABLE [dbo].[ExtractableDataSet] ADD  CONSTRAINT [DF_ExtractableDataSet_DisableExtraction]  DEFAULT ((0)) FOR [DisableExtraction]
GO
ALTER TABLE [dbo].[ExtractionConfiguration] ADD  CONSTRAINT [DF_ExtractionConfiguration_Separator]  DEFAULT (',') FOR [Separator]
GO
ALTER TABLE [dbo].[ExtractionConfiguration] ADD  CONSTRAINT [DF_ExtractionConfiguration_IsReleased]  DEFAULT ((0)) FOR [IsReleased]
GO
ALTER TABLE [dbo].[CohortCustomColumn]  WITH CHECK ADD  CONSTRAINT [FK_CohortCustomColumn_Cohort] FOREIGN KEY([Cohort_ID])
REFERENCES [dbo].[ExtractableCohort] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CohortCustomColumn] CHECK CONSTRAINT [FK_CohortCustomColumn_Cohort]
GO
ALTER TABLE [dbo].[CumulativeExtractionResults]  WITH CHECK ADD  CONSTRAINT [FK_CumulativeExtractionResults_ExtractableDataSet] FOREIGN KEY([ExtractableDataSet_ID])
REFERENCES [dbo].[ExtractableDataSet] ([ID])
GO
ALTER TABLE [dbo].[CumulativeExtractionResults] CHECK CONSTRAINT [FK_CumulativeExtractionResults_ExtractableDataSet]
GO
ALTER TABLE [dbo].[CumulativeExtractionResults]  WITH CHECK ADD  CONSTRAINT [FK_CumulativeExtractionResults_ExtractionConfiguration] FOREIGN KEY([ExtractionConfiguration_ID])
REFERENCES [dbo].[ExtractionConfiguration] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CumulativeExtractionResults] CHECK CONSTRAINT [FK_CumulativeExtractionResults_ExtractionConfiguration]
GO
ALTER TABLE [dbo].[DeployedExtractionFilter]  WITH CHECK ADD  CONSTRAINT [FK_ExtractionFilter_FilterContainer] FOREIGN KEY([FilterContainer_ID])
REFERENCES [dbo].[FilterContainer] ([ID])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[DeployedExtractionFilter] CHECK CONSTRAINT [FK_ExtractionFilter_FilterContainer]
GO
ALTER TABLE [dbo].[DeployedExtractionFilterParameter]  WITH CHECK ADD  CONSTRAINT [FK_ExtractionFilterParameter_ExtractionFilter] FOREIGN KEY([ExtractionFilter_ID])
REFERENCES [dbo].[DeployedExtractionFilter] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DeployedExtractionFilterParameter] CHECK CONSTRAINT [FK_ExtractionFilterParameter_ExtractionFilter]
GO
ALTER TABLE [dbo].[ExtractableColumn]  WITH CHECK ADD  CONSTRAINT [FK_ExtractableColumn_ExtractableDataSet] FOREIGN KEY([ExtractableDataSet_ID])
REFERENCES [dbo].[ExtractableDataSet] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ExtractableColumn] CHECK CONSTRAINT [FK_ExtractableColumn_ExtractableDataSet]
GO
ALTER TABLE [dbo].[ExtractableColumn]  WITH CHECK ADD  CONSTRAINT [FK_ExtractableColumn_ExtractionConfiguration] FOREIGN KEY([ExtractionConfiguration_ID])
REFERENCES [dbo].[ExtractionConfiguration] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ExtractableColumn] CHECK CONSTRAINT [FK_ExtractableColumn_ExtractionConfiguration]
GO
ALTER TABLE [dbo].[ExtractionConfiguration]  WITH CHECK ADD  CONSTRAINT [FK_ExtractionConfiguration_Cohort] FOREIGN KEY([Cohort_ID])
REFERENCES [dbo].[ExtractableCohort] ([ID])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[ExtractionConfiguration] CHECK CONSTRAINT [FK_ExtractionConfiguration_Cohort]
GO
ALTER TABLE [dbo].[ExtractionConfiguration]  WITH CHECK ADD  CONSTRAINT [FK_ExtractionConfiguration_Project] FOREIGN KEY([Project_ID])
REFERENCES [dbo].[Project] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ExtractionConfiguration] CHECK CONSTRAINT [FK_ExtractionConfiguration_Project]
GO
ALTER TABLE [dbo].[FilterContainerSubcontainers]  WITH CHECK ADD  CONSTRAINT [FK_FilterContainerSubcontainers_FilterContainer] FOREIGN KEY([FilterContainer_ParentID])
REFERENCES [dbo].[FilterContainer] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FilterContainerSubcontainers] CHECK CONSTRAINT [FK_FilterContainerSubcontainers_FilterContainer]
GO
ALTER TABLE [dbo].[GlobalExtractionFilterParameter]  WITH CHECK ADD  CONSTRAINT [FK_GlobalExtractionFilterParameter_ExtractionConfiguration] FOREIGN KEY([ExtractionConfiguration_ID])
REFERENCES [dbo].[ExtractionConfiguration] ([ID])
GO
ALTER TABLE [dbo].[GlobalExtractionFilterParameter] CHECK CONSTRAINT [FK_GlobalExtractionFilterParameter_ExtractionConfiguration]
GO
ALTER TABLE [dbo].[Project_DataUser]  WITH CHECK ADD  CONSTRAINT [FK_Project_DataUser_DataUser] FOREIGN KEY([DataUser_ID])
REFERENCES [dbo].[DataUser] ([ID])
GO
ALTER TABLE [dbo].[Project_DataUser] CHECK CONSTRAINT [FK_Project_DataUser_DataUser]
GO
ALTER TABLE [dbo].[Project_DataUser]  WITH CHECK ADD  CONSTRAINT [FK_Project_DataUser_Project] FOREIGN KEY([Project_ID])
REFERENCES [dbo].[Project] ([ID])
GO
ALTER TABLE [dbo].[Project_DataUser] CHECK CONSTRAINT [FK_Project_DataUser_Project]
GO
ALTER TABLE [dbo].[SelectedDataSets]  WITH CHECK ADD  CONSTRAINT [FK_SelectedDataSets_ExtractableDataSet] FOREIGN KEY([ExtractableDataSet_ID])
REFERENCES [dbo].[ExtractableDataSet] ([ID])
GO
ALTER TABLE [dbo].[SelectedDataSets] CHECK CONSTRAINT [FK_SelectedDataSets_ExtractableDataSet]
GO
ALTER TABLE [dbo].[SelectedDataSets]  WITH CHECK ADD  CONSTRAINT [FK_SelectedDataSets_ExtractionConfiguration] FOREIGN KEY([ExtractionConfiguration_ID])
REFERENCES [dbo].[ExtractionConfiguration] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SelectedDataSets] CHECK CONSTRAINT [FK_SelectedDataSets_ExtractionConfiguration]
GO
ALTER TABLE [dbo].[SelectedDataSets]  WITH CHECK ADD  CONSTRAINT [FK_SelectedDataSets_FilterContainer] FOREIGN KEY([RootFilterContainer_ID])
REFERENCES [dbo].[FilterContainer] ([ID])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[SelectedDataSets] CHECK CONSTRAINT [FK_SelectedDataSets_FilterContainer]
GO
USE [master]
GO
ALTER DATABASE [DataExportManager2] SET  READ_WRITE 
GO
