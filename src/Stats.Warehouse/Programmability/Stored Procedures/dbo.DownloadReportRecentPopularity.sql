﻿CREATE PROCEDURE [dbo].[DownloadReportRecentPopularity]
	@ReportGenerationTime DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Cursor DATETIME = (SELECT ISNULL(MAX([Position]), @ReportGenerationTime) FROM [dbo].[Cursors] (NOLOCK) WHERE [Name] = 'GetDirtyPackageId')

	-- Find all packages that have had download facts added in the last 42 days, today inclusive
	SELECT		TOP 100 P.[PackageId]
				,SUM(ISNULL(F.[DownloadCount], 0)) AS 'Downloads'
	FROM		[dbo].[Fact_Download] AS F (NOLOCK)

	INNER JOIN	[dbo].[Dimension_Date] AS D (NOLOCK)
	ON			F.[Dimension_Date_Id] = D.[Id]

	INNER JOIN	[dbo].[Dimension_Package] AS P (NOLOCK)
	ON			F.[Dimension_Package_Id] = P.[Id]

	INNER JOIN	Dimension_Client AS C (NOLOCK)
	ON			C.[Id] = F.[Dimension_Client_Id]

	WHERE		D.[Date] IS NOT NULL
			AND ISNULL(D.[Date], CONVERT(DATE, '1900-01-01')) >= CONVERT(DATE, DATEADD(day, -42, @ReportGenerationTime))
			AND ISNULL(D.[Date], CONVERT(DATE, DATEADD(day, 1, @ReportGenerationTime))) <= CONVERT(DATE, @ReportGenerationTime)
			AND F.[Timestamp] <= @Cursor
			AND C.ClientCategory NOT IN ('Crawler', 'Unknown')
			AND NOT (C.ClientCategory = 'NuGet' AND CAST(ISNULL(C.[Major], '0') AS INT) > 10)

	GROUP BY	P.[PackageId]
	ORDER BY	[Downloads] DESC
END