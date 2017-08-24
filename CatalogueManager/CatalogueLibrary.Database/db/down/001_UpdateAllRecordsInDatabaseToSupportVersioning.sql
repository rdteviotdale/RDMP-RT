alter table ANOTable					DROP CONSTRAINT "df_ANOTable_SoftwareVersion"
alter table AggregateConfiguration		DROP CONSTRAINT "df_AggregateConfiguration_SoftwareVersion"
alter table AggregateDimension			DROP CONSTRAINT "df_AggregateDimension_SoftwareVersion"
alter table AggregateFilter				DROP CONSTRAINT "df_AggregateFilter_SoftwareVersion"
alter table AggregateFilterContainer	DROP CONSTRAINT "df_AggregateFilterContainer_SoftwareVersion"
alter table AggregateFilterParameter	DROP CONSTRAINT "df_AggregateFilterParameter_SoftwareVersion"
alter table Catalogue					DROP CONSTRAINT "df_Catalogue_SoftwareVersion"
alter table CatalogueItem				DROP CONSTRAINT "df_CatalogueItem_SoftwareVersion"
alter table CatalogueItemIssue			DROP CONSTRAINT "df_CatalogueItemIssue_SoftwareVersion"
alter table ColumnInfo					DROP CONSTRAINT "df_ColumnInfo_SoftwareVersion" 
alter table ExternalDatabaseServer		DROP CONSTRAINT "df_ExternalDatabaseServer_SoftwareVersion" 
alter table ExtractionFilter			DROP CONSTRAINT "df_ExtractionFilter_SoftwareVersion"
alter table ExtractionFilterParameter	DROP CONSTRAINT "df_ExtractionFilterParameter_SoftwareVersion"
alter table ExtractionInformation		DROP CONSTRAINT "df_ExtractionInformation_SoftwareVersion"
alter table IssueSystemUser				DROP CONSTRAINT "df_IssueSystemUser_SoftwareVersion" 
alter table JoinInfo					DROP CONSTRAINT "df_JoinInfo_SoftwareVersion" 
alter table LoadMetadata				DROP CONSTRAINT "df_LoadMetadata_SoftwareVersion" 
alter table LoadModuleAssembly			DROP CONSTRAINT "df_LoadModuleAssembly_SoftwareVersion" 
alter table LoadSchedule				DROP CONSTRAINT "df_LoadSchedule_SoftwareVersion" 
alter table Lookup						DROP CONSTRAINT "df_Lookup_SoftwareVersion" 
alter table LookupCompositeJoinInfo		DROP CONSTRAINT "df_LookupCompositeJoinInfo_SoftwareVersion" 
alter table OPCS3_PossibleDescriptions	DROP CONSTRAINT "df_OPCS3_PossibleDescriptions_SoftwareVersion" 
alter table Old_ICD10_Codes				DROP CONSTRAINT "df_Old_ICD10_Codes_SoftwareVersion"
alter table Old_ICD9_Codes				DROP CONSTRAINT "df_Old_ICD9_Codes_SoftwareVersion"
alter table PreLoadDiscardedColumn		DROP CONSTRAINT "df_PreLoadDiscardedColumn_SoftwareVersion"
alter table ProcessTask					DROP CONSTRAINT "df_ProcessTask_SoftwareVersion" 
alter table ProcessTaskArgument			DROP CONSTRAINT "df_ProcessTaskArgument_SoftwareVersion"
alter table SupportingDocument			DROP CONSTRAINT "df_SupportingDocument_SoftwareVersion"
alter table SupportingSQLTable			DROP CONSTRAINT "df_SupportingSQLTable_SoftwareVersion"
alter table TableInfo					DROP CONSTRAINT "df_TableInfo_SoftwareVersion"
GO 

alter table ANOTable					drop column SoftwareVersion
alter table AggregateConfiguration		drop column SoftwareVersion
alter table AggregateDimension			drop column SoftwareVersion
alter table AggregateFilter				drop column SoftwareVersion
alter table AggregateFilterContainer	drop column SoftwareVersion
alter table AggregateFilterParameter	drop column SoftwareVersion
alter table Catalogue					drop column SoftwareVersion
alter table CatalogueItem				drop column SoftwareVersion
alter table CatalogueItemIssue			drop column SoftwareVersion
alter table ColumnInfo					drop column SoftwareVersion
alter table ExternalDatabaseServer		drop column SoftwareVersion
alter table ExtractionFilter			drop column SoftwareVersion
alter table ExtractionFilterParameter	drop column SoftwareVersion
alter table ExtractionInformation		drop column SoftwareVersion
alter table IssueSystemUser				drop column SoftwareVersion
alter table JoinInfo					drop column SoftwareVersion
alter table LoadMetadata				drop column SoftwareVersion
alter table LoadModuleAssembly			drop column SoftwareVersion
alter table LoadSchedule				drop column SoftwareVersion
alter table Lookup						drop column SoftwareVersion
alter table LookupCompositeJoinInfo		drop column SoftwareVersion
alter table OPCS3_PossibleDescriptions	drop column SoftwareVersion
alter table Old_ICD10_Codes				drop column SoftwareVersion
alter table Old_ICD9_Codes				drop column SoftwareVersion
alter table PreLoadDiscardedColumn		drop column SoftwareVersion
alter table ProcessTask					drop column SoftwareVersion
alter table ProcessTaskArgument			drop column SoftwareVersion
alter table SupportingDocument			drop column SoftwareVersion
alter table SupportingSQLTable			drop column SoftwareVersion
alter table TableInfo					drop column SoftwareVersion
GO 

DROP FUNCTION GetSoftwareVersion
GO