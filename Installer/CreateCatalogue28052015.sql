/****** Object:  UserDefinedFunction [dbo].[GetDefaultExternalServerIDFor]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--So we can use it in a DEFAULT constraint
CREATE FUNCTION [dbo].[GetDefaultExternalServerIDFor]
(
	-- Add the parameters for the function here
	@Default varchar(50)
)
RETURNS int
AS
BEGIN
	
	RETURN (SELECT ExternalDatabaseServer_ID from ServerDefaults where DefaultType = @Default)
END

GO
/****** Object:  UserDefinedFunction [dbo].[GetSoftwareVersion]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--todo dbo should be RoundhousE
CREATE FUNCTION [dbo].[GetSoftwareVersion]()
RETURNS nvarchar(50)
AS
BEGIN
	-- Return the result of the function
	RETURN (SELECT '0.0.0.0000')
END

GO
/****** Object:  Table [dbo].[ANOTable]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ANOTable](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](500) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
	[Server_ID] [int] NOT NULL,
 CONSTRAINT [PK_ANOTable] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AggregateConfiguration]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AggregateConfiguration](
	[Catalogue_ID] [int] NOT NULL,
	[Name] [varchar](500) NULL,
	[Description] [varchar](5000) NULL,
	[dtCreated] [datetime] NOT NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[RootFilterContainer_ID] [int] NULL,
	[CountSQL] [varchar](1000) NOT NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
	[PivotOnDimensionID] [int] NULL,
	[IsExtractable] [bit] NOT NULL,
 CONSTRAINT [PK_AggregateConfiguration] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AggregateContinuousDateAxis]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AggregateContinuousDateAxis](
	[AggregateDimension_ID] [int] NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StartDate] [varchar](500) NULL,
	[EndDate] [varchar](500) NULL,
	[AxisIncrement] [int] NOT NULL,
 CONSTRAINT [PK_AggregateContinuousDateAxis] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AggregateDimension]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AggregateDimension](
	[AggregateConfiguration_ID] [int] NOT NULL,
	[ExtractionInformation_ID] [int] NOT NULL,
	[SelectSQL] [varchar](max) NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Alias] [varchar](100) NULL,
	[Order] [int] NOT NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_AggregateDimension] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AggregateFilter]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AggregateFilter](
	[FilterContainer_ID] [int] NULL,
	[WhereSQL] [varchar](max) NULL,
	[Description] [varchar](500) NULL,
	[Name] [varchar](100) NOT NULL,
	[IsMandatory] [bit] NOT NULL,
	[AssociatedColumnInfo_ID] [int] NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_AggregateFilter] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AggregateFilterContainer]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AggregateFilterContainer](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Operation] [varchar](10) NOT NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_AggregateFilterContainer] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AggregateFilterParameter]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AggregateFilterParameter](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AggregateFilter_ID] [int] NOT NULL,
	[ParameterSQL] [varchar](500) NULL,
	[Value] [varchar](500) NULL,
	[Comment] [varchar](500) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_AggregateFilterParameter] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AggregateFilterSubContainer]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AggregateFilterSubContainer](
	[AggregateFilterContainer_ParentID] [int] NULL,
	[AggregateFilterContainer_ChildID] [int] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AggregateForcedJoin]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AggregateForcedJoin](
	[AggregateConfiguration_ID] [int] NOT NULL,
	[TableInfo_ID] [int] NOT NULL,
 CONSTRAINT [PK_AggregateForcedJoin] PRIMARY KEY CLUSTERED 
(
	[AggregateConfiguration_ID] ASC,
	[TableInfo_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Catalogue]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Catalogue](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Acronym] [varchar](50) NULL,
	[Name] [varchar](1000) NULL,
	[Description] [text] NULL,
	[Detail_Page_URL] [varchar](150) NULL,
	[Type] [varchar](50) NULL,
	[Periodicity] [varchar](50) NULL,
	[Geographical_coverage] [varchar](150) NULL,
	[Background_summary] [text] NULL,
	[Search_keywords] [varchar](150) NULL,
	[Update_freq] [varchar](50) NULL,
	[Update_sched] [varchar](50) NULL,
	[Time_coverage] [varchar](50) NULL,
	[Last_revision_date] [date] NULL,
	[Contact_details] [varchar](50) NULL,
	[Resource_owner] [varchar](50) NULL,
	[Attribution_citation] [varchar](500) NULL,
	[Access_options] [varchar](150) NULL,
	[API_access_URL] [varchar](150) NULL,
	[Browse_URL] [varchar](150) NULL,
	[Bulk_Download_URL] [varchar](150) NULL,
	[Query_tool_URL] [varchar](150) NULL,
	[Source_URL] [varchar](150) NULL,
	[Granularity] [varchar](50) NULL,
	[Country_of_origin] [varchar](150) NULL,
	[Data_standards] [varchar](500) NULL,
	[Administrative_contact_name] [varchar](50) NULL,
	[Administrative_contact_email] [varchar](255) NULL,
	[Administrative_contact_telephone] [varchar](50) NULL,
	[Administrative_contact_address] [varchar](500) NULL,
	[Explicit_consent] [bit] NULL,
	[Ethics_approver] [varchar](255) NULL,
	[Source_of_data_collection] [varchar](500) NULL,
	[SubjectNumbers] [varchar](50) NULL,
	[TimeCoverage_ExtractionInformation_ID] [int] NULL,
	[ValidatorXML] [varchar](max) NULL,
	[LoggingDataTask] [varchar](100) NULL,
	[JIRATicket] [varchar](20) NULL,
	[DatasetStartDate] [datetime] NULL,
	[IsDepricated] [bit] NOT NULL,
	[LoadMetadata_ID] [int] NULL,
	[IsInternalDataset] [bit] NOT NULL,
	[LiveLoggingServer_ID] [int] NULL,
	[TestLoggingServer_ID] [int] NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Data_Catalogue] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[CatalogueItem]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CatalogueItem](
	[Catalogue_ID] [int] NOT NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](256) NOT NULL,
	[Statistical_cons] [text] NULL,
	[Research_relevance] [text] NULL,
	[Description] [text] NULL,
	[Topic] [varchar](50) NULL,
	[Periodicity] [varchar](50) NULL,
	[Agg_method] [varchar](255) NULL,
	[Limitations] [text] NULL,
	[Comments] [text] NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Catalogue_Items] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[CatalogueItemIssue]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CatalogueItemIssue](
	[CatalogueItem_ID] [int] NOT NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](500) NULL,
	[Description] [varchar](max) NULL,
	[SQL] [varchar](max) NULL,
	[JIRATicket] [varchar](10) NULL,
	[Status] [varchar](20) NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[UserWhoCreated] [varchar](500) NOT NULL,
	[DateOfLastStatusChange] [datetime] NULL,
	[UserWhoLastChangedStatus] [varchar](500) NULL,
	[Severity] [varchar](100) NOT NULL,
	[ReportedBy_ID] [int] NULL,
	[ReportedOnDate] [datetime] NULL,
	[Owner_ID] [int] NULL,
	[Action] [varchar](max) NULL,
	[NotesToResearcher] [varchar](max) NULL,
	[PathToExcelSheetWithAdditionalInformation] [varchar](1000) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_CatalogueItemIssue] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ColumnInfo]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ColumnInfo](
	[TableInfo_ID] [int] NOT NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Data_type] [varchar](50) NULL,
	[Format] [varchar](50) NULL,
	[Digitisation_specs] [varchar](255) NULL,
	[Name] [varchar](1000) NULL,
	[Source] [varchar](50) NULL,
	[Description] [varchar](1000) NULL,
	[Status] [varchar](10) NULL,
	[RegexPattern] [varchar](255) NULL,
	[ValidationRules] [varchar](5000) NULL,
	[IsPrimaryKey] [bit] NOT NULL,
	[ANOTable_ID] [int] NULL,
	[DuplicateRecordResolutionOrder] [int] NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
	[DuplicateRecordResolutionIsAscending] [bit] NOT NULL,
 CONSTRAINT [PK_Table_Items] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ColumnInfo_CatalogueItem]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ColumnInfo_CatalogueItem](
	[ColumnInfo_ID] [int] NULL,
	[CatalogueItem_ID] [int] NULL,
	[ExtractionInformation_ID] [int] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DLEWindowsServiceException]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DLEWindowsServiceException](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MachineName] [varchar](500) NOT NULL,
	[Exception] [varchar](max) NOT NULL,
	[EventDate] [datetime] NOT NULL,
	[LoadMetadata_ID] [int] NOT NULL,
	[Explanation] [varchar](max) NULL,
 CONSTRAINT [PK_DLEWindowsServiceExceptions] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[DuplicationResolutionOrder]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DuplicationResolutionOrder](
	[ColumnInfo_ID] [int] NULL,
	[ResolveOrder] [int] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ExternalDatabaseServer]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ExternalDatabaseServer](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](500) NULL,
	[DatabaseName] [varchar](50) NULL,
	[ServerName] [varchar](50) NULL,
	[Username] [varchar](50) NULL,
	[Password] [varchar](50) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_ExternalDatabaseServer] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ExtractionFilter]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ExtractionFilter](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ExtractionInformation_ID] [int] NOT NULL,
	[WhereSQL] [varchar](max) NULL,
	[Description] [varchar](500) NULL,
	[Name] [varchar](100) NOT NULL,
	[IsMandatory] [bit] NOT NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_ExtractionFilter] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ExtractionFilterParameter]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ExtractionFilterParameter](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ExtractionFilter_ID] [int] NOT NULL,
	[ParameterSQL] [varchar](500) NULL,
	[Value] [varchar](500) NULL,
	[Comment] [varchar](500) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_ExtractionFilterParameter] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ExtractionInformation]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ExtractionInformation](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SelectSQL] [varchar](max) NOT NULL,
	[Order] [int] NOT NULL,
	[ExtractionCategory] [varchar](30) NOT NULL,
	[Alias] [varchar](100) NULL,
	[HashOnDataRelease] [bit] NOT NULL,
	[IsExtractionIdentifier] [bit] NOT NULL,
	[IsPrimaryKey] [bit] NOT NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_ExtractionInformation] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[IssueSystemUser]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[IssueSystemUser](
	[Name] [varchar](200) NOT NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[EmailAddress] [varchar](500) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_IssueSystemUser] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[JoinInfo]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[JoinInfo](
	[ForeignKey_ID] [int] NOT NULL,
	[PrimaryKey_ID] [int] NOT NULL,
	[ExtractionJoinType] [varchar](100) NOT NULL,
	[Collation] [varchar](50) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_JoinInfo] PRIMARY KEY CLUSTERED 
(
	[ForeignKey_ID] ASC,
	[PrimaryKey_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[LoadMetadata]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[LoadMetadata](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[LocationOfFlatFiles] [varchar](3000) NULL,
	[IncludeDataset] [bit] NOT NULL,
	[UsesStandardisedLoadProcess] [bit] NOT NULL,
	[ScheduleStartDate] [datetime] NULL,
	[SchedulePeriod] [int] NULL,
	[RawDatabaseServer] [varchar](50) NULL,
	[StagingDatabaseServer] [varchar](50) NULL,
	[LiveDatabaseServer] [varchar](50) NULL,
	[AnonymisationEngineClass] [varchar](50) NULL,
	[RawDataSource] [varchar](3000) NULL,
	[Name] [varchar](500) NOT NULL,
	[Description] [varchar](max) NULL,
	[EnableAnonymisation] [bit] NOT NULL,
	[OverrideLoggingServer] [varchar](50) NULL,
	[EnableLookupPopulation] [bit] NOT NULL,
	[EnablePrimaryKeyDuplicationResolution] [bit] NOT NULL,
	[CacheFilenameDateFormat] [varchar](20) NOT NULL,
	[CacheArchiveType] [int] NOT NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_LoadMetadata] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[LoadModuleAssembly]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[LoadModuleAssembly](
	[Name] [varchar](250) NOT NULL,
	[Dll] [varbinary](max) NOT NULL,
	[Description] [varchar](2000) NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Committer] [varchar](2000) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_LoadModuleAssembly] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[LoadPeriodically]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoadPeriodically](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[LoadMetadata_ID] [int] NOT NULL,
	[LastLoaded] [date] NULL,
	[DaysToWaitBetweenLoads] [int] NOT NULL,
	[OnSuccessLaunchLoadMetadata_ID] [int] NULL,
 CONSTRAINT [PK_LoadPeriodically] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LoadSchedule]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[LoadSchedule](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIdentifier] [varchar](500) NOT NULL,
	[Healthboard] [varchar](1) NULL,
	[OriginDate] [datetime] NULL,
	[CacheProgress] [datetime] NULL,
	[DataLoadProgress] [datetime] NULL,
	[LastSuccesfulDataLoadRunID] [int] NULL,
	[LastSuccesfulDataLoadRunIDServer] [varchar](100) NULL,
	[LoadMetadata_ID] [int] NOT NULL,
	[LockedBecauseRunning] [bit] NOT NULL,
	[LockHeldBy] [varchar](100) NULL,
	[LoadPeriodicity] [varchar](10) NOT NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_LoadSchedule] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Lookup]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Lookup](
	[Description_ID] [int] NOT NULL,
	[ForeignKey_ID] [int] NOT NULL,
	[PrimaryKey_ID] [int] NOT NULL,
	[ExtractionJoinType] [varchar](100) NOT NULL,
	[Collation] [varchar](50) NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Lookup] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[LookupCompositeJoinInfo]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[LookupCompositeJoinInfo](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[OriginalLookup_ID] [int] NOT NULL,
	[ForeignKey_ID] [int] NOT NULL,
	[PrimaryKey_ID] [int] NOT NULL,
	[Collation] [varchar](50) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_LookupCompositeJoinInfo] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[OPCS3_PossibleDescriptions]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[OPCS3_PossibleDescriptions](
	[Code] [int] NULL,
	[PossibleDescriptions] [varchar](500) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Old_ICD10_Codes]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Old_ICD10_Codes](
	[CODE] [varchar](5) NULL,
	[SUBCODE] [varchar](2) NULL,
	[DESCRIPTION] [varchar](255) NULL,
	[FullCode] [varchar](10) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Old_ICD9_Codes]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Old_ICD9_Codes](
	[Code] [varchar](5) NULL,
	[SubCode] [varchar](2) NULL,
	[SUBSUBCODE] [varchar](2) NULL,
	[Description] [varchar](255) NULL,
	[FullCode] [varchar](10) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[PreLoadDiscardedColumn]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PreLoadDiscardedColumn](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TableInfo_ID] [int] NOT NULL,
	[Destination] [int] NOT NULL,
	[RuntimeColumnName] [varchar](500) NOT NULL,
	[SqlDataType] [varchar](50) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
	[DuplicateRecordResolutionOrder] [int] NULL,
	[DuplicateRecordResolutionIsAscending] [bit] NOT NULL,
 CONSTRAINT [PK_PreLoadDiscardedColumn] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ProcessTask]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ProcessTask](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[LoadMetadata_ID] [int] NOT NULL,
	[Path] [varchar](500) NULL,
	[ProcessTaskType] [nchar](50) NOT NULL,
	[LoadStage] [nchar](50) NOT NULL,
	[Name] [varchar](500) NOT NULL,
	[Order] [int] NOT NULL,
	[RelatesSolelyToCatalogue_ID] [int] NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
	[IsDisabled] [bit] NOT NULL,
 CONSTRAINT [PK_ProcessTask] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ProcessTaskArgument]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ProcessTaskArgument](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ProcessTask_ID] [int] NOT NULL,
	[Name] [varchar](500) NOT NULL,
	[Value] [varchar](max) NULL,
	[Type] [varchar](500) NOT NULL,
	[Description] [varchar](1000) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_ProcessTaskArgument] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ServerDefaults]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ServerDefaults](
	[DefaultType] [varchar](500) NOT NULL,
	[ExternalDatabaseServer_ID] [int] NULL,
 CONSTRAINT [PK_ServerDefaults] PRIMARY KEY CLUSTERED 
(
	[DefaultType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[SupportingDocument]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SupportingDocument](
	[Catalogue_ID] [int] NOT NULL,
	[URL] [varchar](500) NULL,
	[Description] [varchar](2000) NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[Extractable] [bit] NOT NULL,
	[JIRATicket] [varchar](10) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_SupportingDocument] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[SupportingSQLTable]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SupportingSQLTable](
	[Catalogue_ID] [int] NOT NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Description] [varchar](2000) NULL,
	[Name] [varchar](200) NOT NULL,
	[Extractable] [bit] NOT NULL,
	[SQL] [varchar](8000) NULL,
	[ConnectionString] [varchar](1000) NULL,
	[IsGlobal] [bit] NOT NULL,
	[JIRATicket] [varchar](10) NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_SupportingSQLTable] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[TRUD_ReadCodes]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TRUD_ReadCodes](
	[ReadCode] [varchar](10) NOT NULL,
	[Version] [int] NOT NULL,
	[OriginFilename] [varchar](50) NOT NULL,
	[Column1] [varchar](800) NOT NULL,
	[Column2] [varchar](max) NULL,
	[Column3] [varchar](max) NULL,
	[Column4] [varchar](max) NULL,
	[Column5] [varchar](max) NULL,
	[Column6] [varchar](max) NULL,
	[Column7] [varchar](max) NULL,
	[Column8] [varchar](max) NULL,
	[Column9] [varchar](max) NULL,
	[Column10] [varchar](max) NULL,
	[Column11] [varchar](max) NULL,
	[hic_dataLoadRunID] [int] NOT NULL,
	[hic_validFrom] [datetime] NULL,
 CONSTRAINT [PK_z_TRUD_ReadCodes] PRIMARY KEY CLUSTERED 
(
	[ReadCode] ASC,
	[OriginFilename] ASC,
	[Column1] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[TRUD_ReadCodes_Archive]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TRUD_ReadCodes_Archive](
	[ReadCode] [varchar](10) NOT NULL,
	[Version] [int] NOT NULL,
	[OriginFilename] [varchar](50) NOT NULL,
	[Column1] [varchar](800) NULL,
	[Column2] [varchar](max) NULL,
	[Column3] [varchar](max) NULL,
	[Column4] [varchar](max) NULL,
	[Column5] [varchar](max) NULL,
	[Column6] [varchar](max) NULL,
	[Column7] [varchar](max) NULL,
	[Column8] [varchar](max) NULL,
	[Column9] [varchar](max) NULL,
	[Column10] [varchar](max) NULL,
	[Column11] [varchar](max) NULL,
	[hic_dataLoadRunID] [int] NOT NULL,
	[hic_validFrom] [datetime] NULL,
	[hic_validTo] [datetime] NULL,
	[hic_userID] [varchar](128) NULL,
	[hic_status] [char](1) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[TableInfo]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TableInfo](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Store_type] [varchar](50) NULL,
	[Database_access] [varchar](50) NULL,
	[Database_name] [varchar](500) NULL,
	[Name] [varchar](1000) NULL,
	[State] [varchar](50) NULL,
	[ValidationXml] [nvarchar](max) NULL,
	[IsPrimaryExtractionTable] [bit] NOT NULL,
	[IsTableValuedFunction] [bit] NOT NULL,
	[IdentifierDumpServer_ID] [int] NULL,
	[SoftwareVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Data_Tables] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[z_ISD_HB_Codes]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[z_ISD_HB_Codes](
	[HB_code] [varchar](50) NULL,
	[description] [varchar](50) NULL,
	[ISD_HB_Code] [varchar](50) NULL,
	[ISD_HB_Code_Comment] [varchar](50) NULL,
	[ISD_HB_Code_Valid_From] [varchar](50) NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[z_LocationCodes]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[z_LocationCodes](
	[Location] [nvarchar](255) NULL,
	[Name] [nvarchar](255) NULL,
	[Address_L1] [nvarchar](255) NULL,
	[Address_L2] [nvarchar](255) NULL,
	[Address_L3] [nvarchar](255) NULL,
	[Address_L4] [nvarchar](255) NULL,
	[Address_L5] [nvarchar](255) NULL,
	[Postcode] [nvarchar](255) NULL,
	[Summary_description] [nvarchar](255) NULL,
	[dt_start] [nvarchar](255) NULL,
	[dt_close] [nvarchar](255) NULL,
	[code_of_destination] [nvarchar](255) NULL,
	[GP_Surgery] [float] NULL,
	[SMR00] [float] NULL,
	[SMR01] [float] NULL,
	[SMR02] [float] NULL,
	[SMR04] [float] NULL,
	[SMR06] [float] NULL,
	[SM11] [float] NULL,
	[SMR20] [float] NULL,
	[SMR25] [float] NULL,
	[SMR30] [float] NULL,
	[SMR50] [float] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[z_OPCS3_codes]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[z_OPCS3_codes](
	[Code] [int] NOT NULL,
 CONSTRAINT [PK_z_OPCS3_codes] PRIMARY KEY CLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[z_OPCS4_codes]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[z_OPCS4_codes](
	[Code] [varchar](10) NOT NULL,
	[Description] [varchar](500) NULL,
 CONSTRAINT [PK_z_OPCS4_codes] PRIMARY KEY CLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[z_WHO_ICD10_codes]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[z_WHO_ICD10_codes](
	[Char_Code_Length] [numeric](20, 0) NULL,
	[Terminal_Non_Termenal] [varchar](1) NULL,
	[Node_Type] [varchar](1) NULL,
	[Chapter_Num] [varchar](2) NULL,
	[First_3_Code] [varchar](3) NULL,
	[Code_Dot_Dash] [varchar](6) NULL,
	[Code_Dot] [varchar](10) NULL,
	[Code] [varchar](10) NULL,
	[Description] [varchar](255) NULL,
	[Mortality_1] [varchar](5) NULL,
	[Mortality_2] [varchar](5) NULL,
	[Mortality_3] [varchar](5) NULL,
	[Mortality_4] [varchar](5) NULL,
	[Morbidity] [varchar](5) NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[z_WHO_ICD9_codes]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[z_WHO_ICD9_codes](
	[Code] [nvarchar](10) NULL,
	[Long_Desc] [nvarchar](1000) NULL,
	[Short_Desc] [nvarchar](500) NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[z_readcode_hierarchy]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[z_readcode_hierarchy](
	[FromCode] [varchar](5) NOT NULL,
	[ToCode] [varchar](5) NOT NULL,
	[Number] [int] NULL,
	[hic_dataLoadRunID] [int] NULL,
	[hic_validFrom] [datetime] NULL,
 CONSTRAINT [PK_z_readcode_hierarchy] PRIMARY KEY CLUSTERED 
(
	[FromCode] ASC,
	[ToCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[z_readcode_hierarchy_Archive]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[z_readcode_hierarchy_Archive](
	[FromCode] [varchar](5) NOT NULL,
	[ToCode] [varchar](5) NOT NULL,
	[Number] [int] NULL,
	[hic_dataLoadRunID] [int] NULL,
	[hic_validFrom] [datetime] NULL,
	[hic_validTo] [datetime] NULL,
	[hic_userID] [varchar](128) NULL,
	[hic_status] [char](1) NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[z_readcode_hierarchy_description]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[z_readcode_hierarchy_description](
	[ReadCode] [varchar](5) NOT NULL,
	[Category] [varchar](1) NOT NULL,
	[PreferredTerm] [varchar](200) NULL,
	[PreferredTermLong] [varchar](200) NULL,
	[PreferredTermLongest] [varchar](255) NULL,
	[hic_dataLoadRunID] [int] NULL,
	[hic_validFrom] [datetime] NULL,
 CONSTRAINT [PK_z_readcode_hierarchy_description] PRIMARY KEY CLUSTERED 
(
	[ReadCode] ASC,
	[Category] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[z_readcode_hierarchy_description_Archive]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[z_readcode_hierarchy_description_Archive](
	[ReadCode] [varchar](5) NOT NULL,
	[Category] [varchar](1) NOT NULL,
	[PreferredTerm] [varchar](200) NULL,
	[PreferredTermLong] [varchar](200) NULL,
	[PreferredTermLongest] [varchar](255) NULL,
	[hic_dataLoadRunID] [int] NULL,
	[hic_validFrom] [datetime] NULL,
	[hic_validTo] [datetime] NULL,
	[hic_userID] [varchar](128) NULL,
	[hic_status] [char](1) NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[z_readcode_lexicon]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[z_readcode_lexicon](
	[ReadCode] [varchar](5) NOT NULL,
	[Name] [varchar](35) NULL,
	[Strength] [varchar](15) NULL,
	[Form] [varchar](10) NULL,
	[BNF] [varchar](11) NULL,
	[ATC] [varchar](8) NULL,
	[PrefTerm30] [varchar](30) NULL,
	[PrefTerm60] [varchar](60) NULL,
	[PrefTerm198] [varchar](198) NULL,
	[StatusFlag] [varchar](1) NULL,
	[hic_dataLoadRunID] [int] NULL,
	[hic_validFrom] [datetime] NULL,
 CONSTRAINT [PK__z_readco__C77FBEA59000F3DE] PRIMARY KEY CLUSTERED 
(
	[ReadCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[z_readcode_lexicon_Archive]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[z_readcode_lexicon_Archive](
	[ReadCode] [varchar](5) NOT NULL,
	[Name] [varchar](35) NULL,
	[Strength] [varchar](15) NULL,
	[Form] [varchar](10) NULL,
	[BNF] [varchar](11) NULL,
	[ATC] [varchar](8) NULL,
	[PrefTerm30] [varchar](30) NULL,
	[PrefTerm60] [varchar](60) NULL,
	[PrefTerm198] [varchar](198) NULL,
	[StatusFlag] [varchar](1) NULL,
	[hic_dataLoadRunID] [int] NULL,
	[hic_validFrom] [datetime] NULL,
	[hic_validTo] [datetime] NULL,
	[hic_userID] [varchar](128) NULL,
	[hic_status] [char](1) NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[z_source_death]    Script Date: 14/05/2015 11:08:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[z_source_death](
	[code] [varchar](1) NULL,
	[description] [varchar](50) NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[ANOTable] ADD  CONSTRAINT [DF_ANOTable_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[AggregateConfiguration] ADD  CONSTRAINT [DF_AggregateConfiguration_dtCreated]  DEFAULT (getdate()) FOR [dtCreated]
GO
ALTER TABLE [dbo].[AggregateConfiguration] ADD  CONSTRAINT [DF_AggregateConfiguration_CountSQL]  DEFAULT ('count(*)') FOR [CountSQL]
GO
ALTER TABLE [dbo].[AggregateConfiguration] ADD  CONSTRAINT [DF_AggregateConfiguration_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[AggregateConfiguration] ADD  CONSTRAINT [DF_AggregateConfiguration_IsExtractable]  DEFAULT ((0)) FOR [IsExtractable]
GO
ALTER TABLE [dbo].[AggregateContinuousDateAxis] ADD  CONSTRAINT [DF_AggregateContinuousDateAxis_StartDate]  DEFAULT ('''2001-01-01''') FOR [StartDate]
GO
ALTER TABLE [dbo].[AggregateContinuousDateAxis] ADD  CONSTRAINT [DF_AggregateContinuousDateAxis_EndDate]  DEFAULT ('getdate()') FOR [EndDate]
GO
ALTER TABLE [dbo].[AggregateContinuousDateAxis] ADD  CONSTRAINT [DF_AggregateContinuousDateAxis_AxisIncrement]  DEFAULT ((1)) FOR [AxisIncrement]
GO
ALTER TABLE [dbo].[AggregateDimension] ADD  CONSTRAINT [DF_AggregateDimension_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[AggregateFilter] ADD  CONSTRAINT [DF_AggregateFilter_IsMandatory]  DEFAULT ((0)) FOR [IsMandatory]
GO
ALTER TABLE [dbo].[AggregateFilter] ADD  CONSTRAINT [DF_AggregateFilter_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[AggregateFilterContainer] ADD  CONSTRAINT [DF_AggregateFilterContainer_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[AggregateFilterParameter] ADD  CONSTRAINT [DF_AggregateFilterParameter_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[Catalogue] ADD  CONSTRAINT [DF_Catalogue_IsDepricated]  DEFAULT ((0)) FOR [IsDepricated]
GO
ALTER TABLE [dbo].[Catalogue] ADD  CONSTRAINT [DF_Catalogue_IsInternalDataset]  DEFAULT ((0)) FOR [IsInternalDataset]
GO
ALTER TABLE [dbo].[Catalogue] ADD  CONSTRAINT [df_LiveLoggingServer_ID]  DEFAULT ([dbo].[GetDefaultExternalServerIDFor]('Catalogue.LiveLoggingServer_ID')) FOR [LiveLoggingServer_ID]
GO
ALTER TABLE [dbo].[Catalogue] ADD  CONSTRAINT [df_TestLoggingServer_ID]  DEFAULT ([dbo].[GetDefaultExternalServerIDFor]('Catalogue.TestLoggingServer_ID')) FOR [TestLoggingServer_ID]
GO
ALTER TABLE [dbo].[Catalogue] ADD  CONSTRAINT [DF_Catalogue_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[CatalogueItem] ADD  CONSTRAINT [DF_CatalogueItem_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[CatalogueItemIssue] ADD  CONSTRAINT [DF_CatalogueItemIssue_DateCreated]  DEFAULT (getdate()) FOR [DateCreated]
GO
ALTER TABLE [dbo].[CatalogueItemIssue] ADD  CONSTRAINT [DF_CatalogueItemIssue_Severity]  DEFAULT ('Red') FOR [Severity]
GO
ALTER TABLE [dbo].[CatalogueItemIssue] ADD  CONSTRAINT [DF_CatalogueItemIssue_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[ColumnInfo] ADD  CONSTRAINT [DF_ColumnInfo_IsPrimaryKey]  DEFAULT ((0)) FOR [IsPrimaryKey]
GO
ALTER TABLE [dbo].[ColumnInfo] ADD  CONSTRAINT [DF_ColumnInfo_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[ColumnInfo] ADD  CONSTRAINT [DF_ColumnInfo_DuplicateRecordResolutionIsAscending]  DEFAULT ((1)) FOR [DuplicateRecordResolutionIsAscending]
GO
ALTER TABLE [dbo].[DLEWindowsServiceException] ADD  CONSTRAINT [DF_DLEServiceExceptions_EventDate]  DEFAULT (getdate()) FOR [EventDate]
GO
ALTER TABLE [dbo].[ExternalDatabaseServer] ADD  CONSTRAINT [DF_ExternalDatabaseServer_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[ExtractionFilter] ADD  CONSTRAINT [DF_ExtractionFilter_IsMandatory]  DEFAULT ((0)) FOR [IsMandatory]
GO
ALTER TABLE [dbo].[ExtractionFilter] ADD  CONSTRAINT [DF_ExtractionFilter_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[ExtractionFilterParameter] ADD  CONSTRAINT [DF_ExtractionFilterParameter_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[ExtractionInformation] ADD  CONSTRAINT [DF_ExtractionInformation_HashOnDataRelease]  DEFAULT ((0)) FOR [HashOnDataRelease]
GO
ALTER TABLE [dbo].[ExtractionInformation] ADD  CONSTRAINT [DF_ExtractionInformation_IsExtractionIdentifier]  DEFAULT ((0)) FOR [IsExtractionIdentifier]
GO
ALTER TABLE [dbo].[ExtractionInformation] ADD  CONSTRAINT [DF_ExtractionInformation_IsPrimaryKey]  DEFAULT ((0)) FOR [IsPrimaryKey]
GO
ALTER TABLE [dbo].[ExtractionInformation] ADD  CONSTRAINT [DF_ExtractionInformation_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[IssueSystemUser] ADD  CONSTRAINT [DF_IssueSystemUser_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[JoinInfo] ADD  CONSTRAINT [DF_JoinInfo_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[LoadMetadata] ADD  CONSTRAINT [DF_LoadMetadata_IncludeDataset]  DEFAULT ((1)) FOR [IncludeDataset]
GO
ALTER TABLE [dbo].[LoadMetadata] ADD  CONSTRAINT [DF_LoadMetadata_UsesStandardisedLoadProcess]  DEFAULT ((1)) FOR [UsesStandardisedLoadProcess]
GO
ALTER TABLE [dbo].[LoadMetadata] ADD  CONSTRAINT [DF_LoadMetadata_EnableAnonymisation]  DEFAULT ((0)) FOR [EnableAnonymisation]
GO
ALTER TABLE [dbo].[LoadMetadata] ADD  CONSTRAINT [DF_LoadMetadata_SkipLookups]  DEFAULT ((0)) FOR [EnableLookupPopulation]
GO
ALTER TABLE [dbo].[LoadMetadata] ADD  CONSTRAINT [DF_LoadMetadata_EnablePrimaryKeyDuplicationResolution]  DEFAULT ((0)) FOR [EnablePrimaryKeyDuplicationResolution]
GO
ALTER TABLE [dbo].[LoadMetadata] ADD  CONSTRAINT [DF_LoadMetadata_CacheFilenameDateFormat]  DEFAULT ('yyyy-MM-dd') FOR [CacheFilenameDateFormat]
GO
ALTER TABLE [dbo].[LoadMetadata] ADD  CONSTRAINT [DF_LoadMetadata_CacheArchiveType]  DEFAULT ((0)) FOR [CacheArchiveType]
GO
ALTER TABLE [dbo].[LoadMetadata] ADD  CONSTRAINT [DF_LoadMetadata_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[LoadModuleAssembly] ADD  CONSTRAINT [DF_LoadModuleAssembly_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[LoadSchedule] ADD  CONSTRAINT [DF_LoadSchedule_CachingInProgress]  DEFAULT ((0)) FOR [LockedBecauseRunning]
GO
ALTER TABLE [dbo].[LoadSchedule] ADD  CONSTRAINT [DF_LoadSchedule_LoadPeriodicity]  DEFAULT ((1)) FOR [LoadPeriodicity]
GO
ALTER TABLE [dbo].[LoadSchedule] ADD  CONSTRAINT [DF_LoadSchedule_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[Lookup] ADD  CONSTRAINT [DF_Lookup_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[LookupCompositeJoinInfo] ADD  CONSTRAINT [DF_LookupCompositeJoinInfo_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[OPCS3_PossibleDescriptions] ADD  CONSTRAINT [DF_OPCS3_PossibleDescriptions_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[Old_ICD10_Codes] ADD  CONSTRAINT [DF_Old_ICD10_Codes_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[Old_ICD9_Codes] ADD  CONSTRAINT [DF_Old_ICD9_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[PreLoadDiscardedColumn] ADD  CONSTRAINT [DF_PreLoadDiscardedColumn_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[PreLoadDiscardedColumn] ADD  CONSTRAINT [DF_PreLoadDiscardedColumn_DuplicateRecordResolutionIsAscending]  DEFAULT ((1)) FOR [DuplicateRecordResolutionIsAscending]
GO
ALTER TABLE [dbo].[ProcessTask] ADD  CONSTRAINT [DF_ProcessTask_Order]  DEFAULT ((0)) FOR [Order]
GO
ALTER TABLE [dbo].[ProcessTask] ADD  CONSTRAINT [DF_ProcessTask_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[ProcessTask] ADD  CONSTRAINT [DF_ProcessTask_IsDisabled]  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[ProcessTaskArgument] ADD  CONSTRAINT [DF_ProcessTaskArgument_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[SupportingDocument] ADD  CONSTRAINT [DF_SupportingDocument_Extractable]  DEFAULT ((0)) FOR [Extractable]
GO
ALTER TABLE [dbo].[SupportingDocument] ADD  CONSTRAINT [DF_SupportingDocument_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[SupportingSQLTable] ADD  CONSTRAINT [DF_SupportingSQLTable_Extractable]  DEFAULT ((0)) FOR [Extractable]
GO
ALTER TABLE [dbo].[SupportingSQLTable] ADD  CONSTRAINT [DF_SupportingSQLTable_IsGlobal]  DEFAULT ((0)) FOR [IsGlobal]
GO
ALTER TABLE [dbo].[SupportingSQLTable] ADD  CONSTRAINT [DF_SupportingSQLTable_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[TRUD_ReadCodes] ADD  CONSTRAINT [DF_z_TRUD_ReadCodes_ValidFrom]  DEFAULT (getdate()) FOR [hic_validFrom]
GO
ALTER TABLE [dbo].[TableInfo] ADD  CONSTRAINT [DF_TableInfo_IsPrimaryExtractionTable]  DEFAULT ((0)) FOR [IsPrimaryExtractionTable]
GO
ALTER TABLE [dbo].[TableInfo] ADD  CONSTRAINT [DF_TableInfo_IsTableValuedFunction]  DEFAULT ((0)) FOR [IsTableValuedFunction]
GO
ALTER TABLE [dbo].[TableInfo] ADD  CONSTRAINT [df_IdentifierDumpServer_ID]  DEFAULT ([dbo].[GetDefaultExternalServerIDFor]('TableInfo.IdentifierDumpServer_ID')) FOR [IdentifierDumpServer_ID]
GO
ALTER TABLE [dbo].[TableInfo] ADD  CONSTRAINT [DF_TableInfo_SoftwareVersion]  DEFAULT ([dbo].[GetSoftwareVersion]()) FOR [SoftwareVersion]
GO
ALTER TABLE [dbo].[z_readcode_hierarchy] ADD  CONSTRAINT [DF__z_readcode_hierarchy_ValidFrom]  DEFAULT (getdate()) FOR [hic_validFrom]
GO
ALTER TABLE [dbo].[z_readcode_hierarchy_description] ADD  CONSTRAINT [DF_z_readcode_hierarchy_description_SoftwareVersion]  DEFAULT (getdate()) FOR [hic_validFrom]
GO
ALTER TABLE [dbo].[z_readcode_lexicon] ADD  CONSTRAINT [DF_z_readcode_lexicon_ValidFrom]  DEFAULT (getdate()) FOR [hic_validFrom]
GO
ALTER TABLE [dbo].[ANOTable]  WITH CHECK ADD  CONSTRAINT [FK_ANOTable_ExternalDatabaseServer] FOREIGN KEY([Server_ID])
REFERENCES [dbo].[ExternalDatabaseServer] ([ID])
GO
ALTER TABLE [dbo].[ANOTable] CHECK CONSTRAINT [FK_ANOTable_ExternalDatabaseServer]
GO
ALTER TABLE [dbo].[AggregateConfiguration]  WITH CHECK ADD  CONSTRAINT [FK_AggregateConfiguration_AggregateDimension] FOREIGN KEY([PivotOnDimensionID])
REFERENCES [dbo].[AggregateDimension] ([ID])
GO
ALTER TABLE [dbo].[AggregateConfiguration] CHECK CONSTRAINT [FK_AggregateConfiguration_AggregateDimension]
GO
ALTER TABLE [dbo].[AggregateConfiguration]  WITH CHECK ADD  CONSTRAINT [FK_AggregateConfiguration_AggregateFilterContainer] FOREIGN KEY([RootFilterContainer_ID])
REFERENCES [dbo].[AggregateFilterContainer] ([ID])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[AggregateConfiguration] CHECK CONSTRAINT [FK_AggregateConfiguration_AggregateFilterContainer]
GO
ALTER TABLE [dbo].[AggregateConfiguration]  WITH CHECK ADD  CONSTRAINT [FK_AggregateConfiguration_Catalogue] FOREIGN KEY([Catalogue_ID])
REFERENCES [dbo].[Catalogue] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AggregateConfiguration] CHECK CONSTRAINT [FK_AggregateConfiguration_Catalogue]
GO
ALTER TABLE [dbo].[AggregateContinuousDateAxis]  WITH CHECK ADD  CONSTRAINT [FK_AggregateContinuousDateAxis_AggregateDimension] FOREIGN KEY([AggregateDimension_ID])
REFERENCES [dbo].[AggregateDimension] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AggregateContinuousDateAxis] CHECK CONSTRAINT [FK_AggregateContinuousDateAxis_AggregateDimension]
GO
ALTER TABLE [dbo].[AggregateDimension]  WITH CHECK ADD  CONSTRAINT [FK_AggregateDimension_AggregateConfiguration] FOREIGN KEY([AggregateConfiguration_ID])
REFERENCES [dbo].[AggregateConfiguration] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AggregateDimension] CHECK CONSTRAINT [FK_AggregateDimension_AggregateConfiguration]
GO
ALTER TABLE [dbo].[AggregateDimension]  WITH CHECK ADD  CONSTRAINT [FK_AggregateDimension_ExtractionInformation] FOREIGN KEY([ExtractionInformation_ID])
REFERENCES [dbo].[ExtractionInformation] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AggregateDimension] CHECK CONSTRAINT [FK_AggregateDimension_ExtractionInformation]
GO
ALTER TABLE [dbo].[AggregateFilter]  WITH CHECK ADD  CONSTRAINT [FK_AggregateFilter_AggregateFilterContainer] FOREIGN KEY([FilterContainer_ID])
REFERENCES [dbo].[AggregateFilterContainer] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AggregateFilter] CHECK CONSTRAINT [FK_AggregateFilter_AggregateFilterContainer]
GO
ALTER TABLE [dbo].[AggregateFilterParameter]  WITH CHECK ADD  CONSTRAINT [FK_AggregateFilterParameter_AggregateFilter] FOREIGN KEY([AggregateFilter_ID])
REFERENCES [dbo].[AggregateFilter] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AggregateFilterParameter] CHECK CONSTRAINT [FK_AggregateFilterParameter_AggregateFilter]
GO
ALTER TABLE [dbo].[AggregateFilterSubContainer]  WITH CHECK ADD  CONSTRAINT [FK_AggregateFilterSubContainer_AggregateFilterContainer] FOREIGN KEY([AggregateFilterContainer_ParentID])
REFERENCES [dbo].[AggregateFilterContainer] ([ID])
GO
ALTER TABLE [dbo].[AggregateFilterSubContainer] CHECK CONSTRAINT [FK_AggregateFilterSubContainer_AggregateFilterContainer]
GO
ALTER TABLE [dbo].[AggregateFilterSubContainer]  WITH CHECK ADD  CONSTRAINT [FK_AggregateFilterSubContainer_AggregateFilterContainer1] FOREIGN KEY([AggregateFilterContainer_ChildID])
REFERENCES [dbo].[AggregateFilterContainer] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AggregateFilterSubContainer] CHECK CONSTRAINT [FK_AggregateFilterSubContainer_AggregateFilterContainer1]
GO
ALTER TABLE [dbo].[AggregateForcedJoin]  WITH CHECK ADD  CONSTRAINT [FK_AggregateForcedJoin_AggregateConfiguration] FOREIGN KEY([AggregateConfiguration_ID])
REFERENCES [dbo].[AggregateConfiguration] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AggregateForcedJoin] CHECK CONSTRAINT [FK_AggregateForcedJoin_AggregateConfiguration]
GO
ALTER TABLE [dbo].[AggregateForcedJoin]  WITH CHECK ADD  CONSTRAINT [FK_AggregateForcedJoin_TableInfo] FOREIGN KEY([TableInfo_ID])
REFERENCES [dbo].[TableInfo] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AggregateForcedJoin] CHECK CONSTRAINT [FK_AggregateForcedJoin_TableInfo]
GO
ALTER TABLE [dbo].[Catalogue]  WITH CHECK ADD  CONSTRAINT [FK_Catalogue_ExternalDatabaseServer] FOREIGN KEY([LiveLoggingServer_ID])
REFERENCES [dbo].[ExternalDatabaseServer] ([ID])
GO
ALTER TABLE [dbo].[Catalogue] CHECK CONSTRAINT [FK_Catalogue_ExternalDatabaseServer]
GO
ALTER TABLE [dbo].[Catalogue]  WITH CHECK ADD  CONSTRAINT [FK_Catalogue_ExternalDatabaseServer1] FOREIGN KEY([TestLoggingServer_ID])
REFERENCES [dbo].[ExternalDatabaseServer] ([ID])
GO
ALTER TABLE [dbo].[Catalogue] CHECK CONSTRAINT [FK_Catalogue_ExternalDatabaseServer1]
GO
ALTER TABLE [dbo].[Catalogue]  WITH CHECK ADD  CONSTRAINT [FK_Catalogue_ExtractionInformation] FOREIGN KEY([TimeCoverage_ExtractionInformation_ID])
REFERENCES [dbo].[ExtractionInformation] ([ID])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Catalogue] CHECK CONSTRAINT [FK_Catalogue_ExtractionInformation]
GO
ALTER TABLE [dbo].[Catalogue]  WITH CHECK ADD  CONSTRAINT [FK_Catalogue_LoadMetadata] FOREIGN KEY([LoadMetadata_ID])
REFERENCES [dbo].[LoadMetadata] ([ID])
GO
ALTER TABLE [dbo].[Catalogue] CHECK CONSTRAINT [FK_Catalogue_LoadMetadata]
GO
ALTER TABLE [dbo].[CatalogueItem]  WITH CHECK ADD  CONSTRAINT [FK_Catalogue_Items_Data_Catalogue] FOREIGN KEY([Catalogue_ID])
REFERENCES [dbo].[Catalogue] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CatalogueItem] CHECK CONSTRAINT [FK_Catalogue_Items_Data_Catalogue]
GO
ALTER TABLE [dbo].[CatalogueItemIssue]  WITH CHECK ADD  CONSTRAINT [FK_CatalogueItemIssue_CatalogueItem] FOREIGN KEY([CatalogueItem_ID])
REFERENCES [dbo].[CatalogueItem] ([ID])
GO
ALTER TABLE [dbo].[CatalogueItemIssue] CHECK CONSTRAINT [FK_CatalogueItemIssue_CatalogueItem]
GO
ALTER TABLE [dbo].[CatalogueItemIssue]  WITH CHECK ADD  CONSTRAINT [FK_CatalogueItemIssue_Owner_IssueSystemUser] FOREIGN KEY([Owner_ID])
REFERENCES [dbo].[IssueSystemUser] ([ID])
GO
ALTER TABLE [dbo].[CatalogueItemIssue] CHECK CONSTRAINT [FK_CatalogueItemIssue_Owner_IssueSystemUser]
GO
ALTER TABLE [dbo].[CatalogueItemIssue]  WITH CHECK ADD  CONSTRAINT [FK_CatalogueItemIssue_Reporter_IssueSystemUser] FOREIGN KEY([ReportedBy_ID])
REFERENCES [dbo].[IssueSystemUser] ([ID])
GO
ALTER TABLE [dbo].[CatalogueItemIssue] CHECK CONSTRAINT [FK_CatalogueItemIssue_Reporter_IssueSystemUser]
GO
ALTER TABLE [dbo].[ColumnInfo]  WITH CHECK ADD  CONSTRAINT [FK_ColumnInfo_ANOTable] FOREIGN KEY([ANOTable_ID])
REFERENCES [dbo].[ANOTable] ([ID])
GO
ALTER TABLE [dbo].[ColumnInfo] CHECK CONSTRAINT [FK_ColumnInfo_ANOTable]
GO
ALTER TABLE [dbo].[ColumnInfo]  WITH CHECK ADD  CONSTRAINT [FK_Table_Items_Data_Tables] FOREIGN KEY([TableInfo_ID])
REFERENCES [dbo].[TableInfo] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ColumnInfo] CHECK CONSTRAINT [FK_Table_Items_Data_Tables]
GO
ALTER TABLE [dbo].[ColumnInfo_CatalogueItem]  WITH CHECK ADD  CONSTRAINT [FK_ColumnInfo_CatalogueItem_CatalogueItem] FOREIGN KEY([CatalogueItem_ID])
REFERENCES [dbo].[CatalogueItem] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ColumnInfo_CatalogueItem] CHECK CONSTRAINT [FK_ColumnInfo_CatalogueItem_CatalogueItem]
GO
ALTER TABLE [dbo].[ColumnInfo_CatalogueItem]  WITH CHECK ADD  CONSTRAINT [FK_ColumnInfo_CatalogueItem_ColumnInfo] FOREIGN KEY([ColumnInfo_ID])
REFERENCES [dbo].[ColumnInfo] ([ID])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[ColumnInfo_CatalogueItem] CHECK CONSTRAINT [FK_ColumnInfo_CatalogueItem_ColumnInfo]
GO
ALTER TABLE [dbo].[ColumnInfo_CatalogueItem]  WITH CHECK ADD  CONSTRAINT [FK_ColumnInfo_CatalogueItem_ExtractionInformation] FOREIGN KEY([ExtractionInformation_ID])
REFERENCES [dbo].[ExtractionInformation] ([ID])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[ColumnInfo_CatalogueItem] CHECK CONSTRAINT [FK_ColumnInfo_CatalogueItem_ExtractionInformation]
GO
ALTER TABLE [dbo].[DLEWindowsServiceException]  WITH CHECK ADD  CONSTRAINT [FK_DLEWindowsServiceException_LoadMetadata] FOREIGN KEY([LoadMetadata_ID])
REFERENCES [dbo].[LoadMetadata] ([ID])
GO
ALTER TABLE [dbo].[DLEWindowsServiceException] CHECK CONSTRAINT [FK_DLEWindowsServiceException_LoadMetadata]
GO
ALTER TABLE [dbo].[ExtractionFilter]  WITH CHECK ADD  CONSTRAINT [FK_ExtractionFilter_ExtractionInformation] FOREIGN KEY([ExtractionInformation_ID])
REFERENCES [dbo].[ExtractionInformation] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ExtractionFilter] CHECK CONSTRAINT [FK_ExtractionFilter_ExtractionInformation]
GO
ALTER TABLE [dbo].[ExtractionFilterParameter]  WITH CHECK ADD  CONSTRAINT [FK_ExtractionFilterParameter_ExtractionFilter] FOREIGN KEY([ExtractionFilter_ID])
REFERENCES [dbo].[ExtractionFilter] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ExtractionFilterParameter] CHECK CONSTRAINT [FK_ExtractionFilterParameter_ExtractionFilter]
GO
ALTER TABLE [dbo].[JoinInfo]  WITH CHECK ADD  CONSTRAINT [FK_JoinInfo_ColumnInfo_JoinKey1] FOREIGN KEY([ForeignKey_ID])
REFERENCES [dbo].[ColumnInfo] ([ID])
GO
ALTER TABLE [dbo].[JoinInfo] CHECK CONSTRAINT [FK_JoinInfo_ColumnInfo_JoinKey1]
GO
ALTER TABLE [dbo].[JoinInfo]  WITH CHECK ADD  CONSTRAINT [FK_JoinInfo_ColumnInfo_JoinKey2] FOREIGN KEY([PrimaryKey_ID])
REFERENCES [dbo].[ColumnInfo] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[JoinInfo] CHECK CONSTRAINT [FK_JoinInfo_ColumnInfo_JoinKey2]
GO
ALTER TABLE [dbo].[LoadPeriodically]  WITH CHECK ADD  CONSTRAINT [FK_LoadPeriodically_LoadMetadata] FOREIGN KEY([LoadMetadata_ID])
REFERENCES [dbo].[LoadMetadata] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LoadPeriodically] CHECK CONSTRAINT [FK_LoadPeriodically_LoadMetadata]
GO
ALTER TABLE [dbo].[LoadPeriodically]  WITH CHECK ADD  CONSTRAINT [FK_LoadPeriodically_LoadMetadata1] FOREIGN KEY([OnSuccessLaunchLoadMetadata_ID])
REFERENCES [dbo].[LoadMetadata] ([ID])
GO
ALTER TABLE [dbo].[LoadPeriodically] CHECK CONSTRAINT [FK_LoadPeriodically_LoadMetadata1]
GO
ALTER TABLE [dbo].[LoadSchedule]  WITH CHECK ADD  CONSTRAINT [FK_LoadSchedule_LoadMetadata] FOREIGN KEY([LoadMetadata_ID])
REFERENCES [dbo].[LoadMetadata] ([ID])
GO
ALTER TABLE [dbo].[LoadSchedule] CHECK CONSTRAINT [FK_LoadSchedule_LoadMetadata]
GO
ALTER TABLE [dbo].[Lookup]  WITH CHECK ADD  CONSTRAINT [FK_Lookup_ColumnInfo] FOREIGN KEY([Description_ID])
REFERENCES [dbo].[ColumnInfo] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Lookup] CHECK CONSTRAINT [FK_Lookup_ColumnInfo]
GO
ALTER TABLE [dbo].[Lookup]  WITH CHECK ADD  CONSTRAINT [FK_Lookup_ColumnInfo1] FOREIGN KEY([ForeignKey_ID])
REFERENCES [dbo].[ColumnInfo] ([ID])
GO
ALTER TABLE [dbo].[Lookup] CHECK CONSTRAINT [FK_Lookup_ColumnInfo1]
GO
ALTER TABLE [dbo].[Lookup]  WITH CHECK ADD  CONSTRAINT [FK_Lookup_ColumnInfo2] FOREIGN KEY([PrimaryKey_ID])
REFERENCES [dbo].[ColumnInfo] ([ID])
GO
ALTER TABLE [dbo].[Lookup] CHECK CONSTRAINT [FK_Lookup_ColumnInfo2]
GO
ALTER TABLE [dbo].[LookupCompositeJoinInfo]  WITH CHECK ADD  CONSTRAINT [FK_LookupCompositeJoinInfo_ColumnInfo] FOREIGN KEY([PrimaryKey_ID])
REFERENCES [dbo].[ColumnInfo] ([ID])
GO
ALTER TABLE [dbo].[LookupCompositeJoinInfo] CHECK CONSTRAINT [FK_LookupCompositeJoinInfo_ColumnInfo]
GO
ALTER TABLE [dbo].[LookupCompositeJoinInfo]  WITH CHECK ADD  CONSTRAINT [FK_LookupCompositeJoinInfo_ColumnInfo_FK] FOREIGN KEY([ForeignKey_ID])
REFERENCES [dbo].[ColumnInfo] ([ID])
GO
ALTER TABLE [dbo].[LookupCompositeJoinInfo] CHECK CONSTRAINT [FK_LookupCompositeJoinInfo_ColumnInfo_FK]
GO
ALTER TABLE [dbo].[LookupCompositeJoinInfo]  WITH CHECK ADD  CONSTRAINT [FK_LookupCompositeJoinInfo_Lookup] FOREIGN KEY([OriginalLookup_ID])
REFERENCES [dbo].[Lookup] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LookupCompositeJoinInfo] CHECK CONSTRAINT [FK_LookupCompositeJoinInfo_Lookup]
GO
ALTER TABLE [dbo].[OPCS3_PossibleDescriptions]  WITH NOCHECK ADD  CONSTRAINT [FK_OPCS3_PossibleDescriptions_z_OPCS3_codes] FOREIGN KEY([Code])
REFERENCES [dbo].[z_OPCS3_codes] ([Code])
GO
ALTER TABLE [dbo].[OPCS3_PossibleDescriptions] CHECK CONSTRAINT [FK_OPCS3_PossibleDescriptions_z_OPCS3_codes]
GO
ALTER TABLE [dbo].[PreLoadDiscardedColumn]  WITH CHECK ADD  CONSTRAINT [FK_PreLoadDiscardedColumn_TableInfo] FOREIGN KEY([TableInfo_ID])
REFERENCES [dbo].[TableInfo] ([ID])
GO
ALTER TABLE [dbo].[PreLoadDiscardedColumn] CHECK CONSTRAINT [FK_PreLoadDiscardedColumn_TableInfo]
GO
ALTER TABLE [dbo].[ProcessTask]  WITH CHECK ADD  CONSTRAINT [FK_ProcessTask_Catalogue] FOREIGN KEY([RelatesSolelyToCatalogue_ID])
REFERENCES [dbo].[Catalogue] ([ID])
GO
ALTER TABLE [dbo].[ProcessTask] CHECK CONSTRAINT [FK_ProcessTask_Catalogue]
GO
ALTER TABLE [dbo].[ProcessTask]  WITH CHECK ADD  CONSTRAINT [FK_ProcessTask_LoadMetadata] FOREIGN KEY([LoadMetadata_ID])
REFERENCES [dbo].[LoadMetadata] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ProcessTask] CHECK CONSTRAINT [FK_ProcessTask_LoadMetadata]
GO
ALTER TABLE [dbo].[ProcessTaskArgument]  WITH CHECK ADD  CONSTRAINT [FK_ProcessTaskArgument_ProcessTask] FOREIGN KEY([ProcessTask_ID])
REFERENCES [dbo].[ProcessTask] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ProcessTaskArgument] CHECK CONSTRAINT [FK_ProcessTaskArgument_ProcessTask]
GO
ALTER TABLE [dbo].[ServerDefaults]  WITH CHECK ADD  CONSTRAINT [FK_ServerDefaults_ExternalDatabaseServer] FOREIGN KEY([ExternalDatabaseServer_ID])
REFERENCES [dbo].[ExternalDatabaseServer] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ServerDefaults] CHECK CONSTRAINT [FK_ServerDefaults_ExternalDatabaseServer]
GO
ALTER TABLE [dbo].[SupportingDocument]  WITH CHECK ADD  CONSTRAINT [FK_SupportingDocument_Catalogue] FOREIGN KEY([Catalogue_ID])
REFERENCES [dbo].[Catalogue] ([ID])
GO
ALTER TABLE [dbo].[SupportingDocument] CHECK CONSTRAINT [FK_SupportingDocument_Catalogue]
GO
ALTER TABLE [dbo].[SupportingSQLTable]  WITH CHECK ADD  CONSTRAINT [FK_SupportingSQLTable_Catalogue] FOREIGN KEY([Catalogue_ID])
REFERENCES [dbo].[Catalogue] ([ID])
GO
ALTER TABLE [dbo].[SupportingSQLTable] CHECK CONSTRAINT [FK_SupportingSQLTable_Catalogue]
GO
ALTER TABLE [dbo].[TableInfo]  WITH CHECK ADD  CONSTRAINT [FK_TableInfo_ExternalDatabaseServer] FOREIGN KEY([IdentifierDumpServer_ID])
REFERENCES [dbo].[ExternalDatabaseServer] ([ID])
GO
ALTER TABLE [dbo].[TableInfo] CHECK CONSTRAINT [FK_TableInfo_ExternalDatabaseServer]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Table ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'‘SMR01’ for example' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Acronym'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Fully expanded name or formula title' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'A description of the data in non-technical terms' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Description'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Link to page describing and explaining the data' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Detail_Page_URL'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Time series, survey, cross section, geospatial, lab etc...' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Type'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sample period for data: Annual, qaurter, Month ...etc' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Periodicity'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Geographical: EU, UK, Scotland, Tayside etc...' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Geographical_coverage'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Any notes on the limitation, derviations or characteristics of the data of potential interest to the users or that the complier/ curator feels guilty about' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Background_summary'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Specific subject that the data deals with' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Search_keywords'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Period of referesh: Biannual, hourly, no fixed schedule...etc' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Update_freq'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date if bext referesh: month, week, or exact day' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Update_sched'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date range for available data in years (1989-2013)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Time_coverage'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Exact date on last data refersh input data' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Last_revision_date'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Email/URL to contact data owner' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Contact_details'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Organisation/Institution from which data originated' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Resource_owner'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Text to use when crediting data source in articles' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Attribution_citation'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'How can data be accessed? API, Bulk download, Query tool, Bespoke Application ...etc' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Access_options'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'URL for SOA access to data for programmatic query' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'API_access_URL'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'URL to data display tool with filter and display option' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Browse_URL'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'URL for data bulk download' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Bulk_Download_URL'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'URL to data explorer and cohort indentifier tool' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Query_tool_URL'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'URL to use when crediting data source in articles' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Catalogue', @level2type=N'COLUMN',@level2name=N'Source_URL'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Unique catalogue entry number as an integer ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CatalogueItem', @level2type=N'COLUMN',@level2name=N'Catalogue_ID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Unique Catalogue entry number as an integer ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CatalogueItem', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Unique admin identifier for the item in the catalogue' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CatalogueItem', @level2type=N'COLUMN',@level2name=N'Name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Basis on which the data item was collected, observed or calculated' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CatalogueItem', @level2type=N'COLUMN',@level2name=N'Statistical_cons'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Why this item in the data set?' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CatalogueItem', @level2type=N'COLUMN',@level2name=N'Research_relevance'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Definition of the concept the item represents' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CatalogueItem', @level2type=N'COLUMN',@level2name=N'Description'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Subject area that the data item belongs to' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CatalogueItem', @level2type=N'COLUMN',@level2name=N'Topic'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sample period for item: Annual, Quarter, Month, etc' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CatalogueItem', @level2type=N'COLUMN',@level2name=N'Periodicity'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'If the item ios aggregated, how was it done?' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CatalogueItem', @level2type=N'COLUMN',@level2name=N'Agg_method'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Caveats in data item collection, observation, processing of interest to users.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CatalogueItem', @level2type=N'COLUMN',@level2name=N'Limitations'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'General comments or any other obesrvations' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CatalogueItem', @level2type=N'COLUMN',@level2name=N'Comments'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Link Item to data catalogue' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CatalogueItem', @level2type=N'CONSTRAINT',@level2name=N'FK_Catalogue_Items_Data_Catalogue'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Database data type of the column' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ColumnInfo', @level2type=N'COLUMN',@level2name=N'Data_type'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Coding technique used to generate data item' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ColumnInfo', @level2type=N'COLUMN',@level2name=N'Digitisation_specs'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Loaded from input, drived, transformed ..etc' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ColumnInfo', @level2type=N'COLUMN',@level2name=N'Source'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SQL DB, NoSQL DB, Hadopp...etc' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TableInfo', @level2type=N'COLUMN',@level2name=N'Store_type'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'IP Address:Port number, server name ..etc' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TableInfo', @level2type=N'COLUMN',@level2name=N'Database_access'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Name of database where the table is' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TableInfo', @level2type=N'COLUMN',@level2name=N'Database_name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Table name' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TableInfo', @level2type=N'COLUMN',@level2name=N'Name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Active, inactive, archived...etc' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TableInfo', @level2type=N'COLUMN',@level2name=N'State'
GO
