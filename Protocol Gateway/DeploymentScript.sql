USE [SdnIoTFuncTest]
GO
/****** Object:  Table [dbo].[ProtoVersion]    Script Date: 2/22/2016 4:38:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProtoVersion](
	[ProtoVersionId] [int] IDENTITY(1,1) NOT NULL,
	[SdnProtoVersion] [nvarchar](50) NOT NULL,
	[VddProtoVersion] [nvarchar](50) NOT NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NULL,
	[LastModifiedBy] [nvarchar](50) NULL,
	[LastModifiedDate] [datetime] NULL,
 CONSTRAINT [PK_dbo.ProtoVersion] PRIMARY KEY CLUSTERED 
(
	[ProtoVersionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
 CONSTRAINT [UK_ProtoVersion_SdnProtoVersion] UNIQUE NONCLUSTERED 
(
	[SdnProtoVersion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
 CONSTRAINT [UK_ProtoVersion_VddProtoVersion] UNIQUE NONCLUSTERED 
(
	[VddProtoVersion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[SdnLogs]    Script Date: 2/22/2016 4:38:34 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SdnLogs](
	[message] [nvarchar](4000) NULL,
	[machinename] [nvarchar](100) NULL,
	[role] [nvarchar](100) NULL,
	[instance] [nvarchar](100) NULL,
	[esn] [nvarchar](100) NULL,
	[messageId] [nvarchar](100) NULL,
	[exception] [nvarchar](4000) NULL,
	[executiondatetime] [datetime] NULL
)

GO
/****** Object:  Table [dbo].[Tcu]    Script Date: 2/22/2016 4:38:35 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tcu](
	[Esn] [nvarchar](8) NOT NULL,
	[Imei] [nvarchar](50) NOT NULL,
	[SimImsi] [nvarchar](15) NULL,
	[TcuAssemblyPartNumber] [nvarchar](max) NOT NULL,
	[TcuHardwarePartNumber] [nvarchar](max) NOT NULL,
	[FirmwareVersion] [nvarchar](50) NULL,
	[FirmwareVersionTeseo2] [nvarchar](50) NULL,
	[FirmwareVersionHe920] [nvarchar](50) NULL,
	[StrategyPartNumber] [nvarchar](50) NULL,
	[ConfigPartNumber] [nvarchar](50) NULL,
	[GlobalConfigVersion] [nvarchar](50) NULL,
	[CanDatabaseVersion] [nvarchar](50) NULL,
	[BusArchitecture] [int] NULL,
	[SupplierRegionId] [int] NOT NULL,
	[ProvisioningRegionId] [int] NULL,
	[PlantShipDate] [date] NOT NULL,
	[IccId] [nvarchar](50) NOT NULL,
	[Msisdn] [nvarchar](50) NULL,
	[ProtoVersionId] [int] NOT NULL,
	[MfgFacilityId] [int] NOT NULL,
	[ProcessorType] [nvarchar](max) NULL,
	[PsKeyHash] [nvarchar](38) NULL,
	[PsKey] [nvarchar](500) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NULL,
	[LastModifiedBy] [nvarchar](50) NULL,
	[LastModifiedDate] [datetime] NULL,
 CONSTRAINT [PK_dbo.Tcu] PRIMARY KEY CLUSTERED 
(
	[Esn] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
 CONSTRAINT [UK_Tcu_IccId] UNIQUE NONCLUSTERED 
(
	[IccId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
 CONSTRAINT [UK_Tcu_Imei] UNIQUE NONCLUSTERED 
(
	[Imei] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[TcuCommand]    Script Date: 2/22/2016 4:38:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TcuCommand](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[esn] [varchar](20) NULL,
	[vin] [varchar](35) NULL,
	[CommandType] [varchar](50) NULL,
	[MessageId] [varchar](50) NULL,
	[CorrelationId] [varchar](50) NULL,
	[PublishedOn] [datetime] NULL,
	[CreatedDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[TcuMessage]    Script Date: 2/22/2016 4:38:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TcuMessage](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TcuVehicleId] [bigint] NOT NULL,
	[MessageType] [nvarchar](50) NOT NULL,
	[MessageSubType] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NULL,
	[MessageId] [nvarchar](50) NULL,
	[CorrelationId] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[TcuVehicle]    Script Date: 2/22/2016 4:38:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TcuVehicle](
	[TcuVehicleId] [bigint] IDENTITY(1,1) NOT NULL,
	[Esn] [nvarchar](8) NOT NULL,
	[Vin] [nvarchar](35) NOT NULL,
	[ActiveTo] [datetime] NULL,
	[ActiveFrom] [datetime] NULL,
	[TcuAuthStatusId] [int] NOT NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NULL,
	[LastModifiedBy] [nvarchar](50) NULL,
	[LastModifiedDate] [datetime] NULL,
 CONSTRAINT [PK_dbo.TcuVehicle] PRIMARY KEY CLUSTERED 
(
	[TcuVehicleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[Vehicle]    Script Date: 2/22/2016 4:38:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Vehicle](
	[Vin] [nvarchar](35) NOT NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NULL,
	[LastModifiedBy] [nvarchar](50) NULL,
	[LastModifiedDate] [datetime] NULL,
	[VehicleTypeId] [bigint] NULL,
	[Series] [varchar](40) NULL,
	[Fuel] [varchar](50) NULL,
	[Displacement] [varchar](50) NULL,
	[Transmission] [varchar](10) NULL,
	[Emission] [varchar](50) NULL,
	[Color] [varchar](50) NULL,
	[Market] [varchar](50) NULL,
	[IsTakeoverInProgress] [bit] NULL,
	[FirstProvisionedOn] [datetime] NULL,
	[SyncGeneration] [nvarchar](2048) NULL,
 CONSTRAINT [PK_dbo.Vehicle] PRIMARY KEY CLUSTERED 
(
	[Vin] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[TcuMessage]  WITH CHECK ADD FOREIGN KEY([TcuVehicleId])
REFERENCES [dbo].[TcuVehicle] ([TcuVehicleId])
GO
ALTER TABLE [dbo].[TcuVehicle]  WITH CHECK ADD  CONSTRAINT [FK_dbo.TcuVehicle_dbo.Tcu_Esn] FOREIGN KEY([Esn])
REFERENCES [dbo].[Tcu] ([Esn])
GO
ALTER TABLE [dbo].[TcuVehicle] CHECK CONSTRAINT [FK_dbo.TcuVehicle_dbo.Tcu_Esn]
GO
ALTER TABLE [dbo].[TcuVehicle]  WITH CHECK ADD  CONSTRAINT [FK_dbo.TcuVehicle_dbo.Vehicle_Vin] FOREIGN KEY([Vin])
REFERENCES [dbo].[Vehicle] ([Vin])
GO
ALTER TABLE [dbo].[TcuVehicle] CHECK CONSTRAINT [FK_dbo.TcuVehicle_dbo.Vehicle_Vin]
GO
/****** Object:  StoredProcedure [dbo].[InsertOrUdpateTcu]    Script Date: 2/22/2016 4:38:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Procedure [dbo].[InsertOrUdpateTcu]
(
    @Esn nvarchar(8) ,
	@Imei nvarchar(50) ,
	@SimImsi nvarchar(15) = NULL,
	@TcuAssemblyPartNumber nvarchar(max) ,
	@TcuHardwarePartNumber nvarchar(max) ,
	@FirmwareVersion nvarchar(50) = NULL,
	@FirmwareVersionTeseo2 nvarchar(50) = NULL,
	@FirmwareVersionHe920 nvarchar(50) = NULL,
	@StrategyPartNumber nvarchar(50) = NULL,
	@ConfigPartNumber nvarchar(50) = NULL,
	@GlobalConfigVersion nvarchar(50) = NULL,
	@CanDatabaseVersion nvarchar(50) = NULL,
	@BusArchitecture int = NULL,
	@SupplierRegionId int ,
	@ProvisioningRegionId int = NULL,
	@PlantShipDate date ,
	@IccId nvarchar(50) ,
	@Msisdn nvarchar(50) = NULL,
	@ProtoVersionId int ,
	@MfgFacilityId int ,
	@ProcessorType nvarchar(max) = NULL,
	@PsKeyHash nvarchar(38) = NULL,
	@PsKey nvarchar(500) ,
	@IsActive bit ,
	@CreatedBy nvarchar(50) = NULL,
	@CreatedDate datetime = NULL,
	@LastModifiedBy nvarchar(50) = NULL,
	@LastModifiedDate datetime = NULL
)
As
Begin
IF Not exists(Select Esn from Tcu where Esn = @Esn)
Begin

INSERT INTO [dbo].[Tcu]
           ([Esn]
           ,[Imei]
           ,[SimImsi]
           ,[TcuAssemblyPartNumber]
           ,[TcuHardwarePartNumber]
           ,[FirmwareVersion]
           ,[FirmwareVersionTeseo2]
           ,[FirmwareVersionHe920]
           ,[StrategyPartNumber]
           ,[ConfigPartNumber]
           ,[GlobalConfigVersion]
           ,[CanDatabaseVersion]
           ,[BusArchitecture]
           ,[SupplierRegionId]
           ,[ProvisioningRegionId]
           ,[PlantShipDate]
           ,[IccId]
           ,[Msisdn]
           ,[ProtoVersionId]
           ,[MfgFacilityId]
           ,[ProcessorType]
           ,[PsKeyHash]
           ,[PsKey]
           ,[IsActive]
           ,[CreatedBy]
           ,[CreatedDate]
           ,[LastModifiedBy]
           ,[LastModifiedDate])
     VALUES
            (@Esn
           ,@Imei
           ,@SimImsi
           ,@TcuAssemblyPartNumber
           ,@TcuHardwarePartNumber
           ,@FirmwareVersion
           ,@FirmwareVersionTeseo2
           ,@FirmwareVersionHe920
           ,@StrategyPartNumber
           ,@ConfigPartNumber
           ,@GlobalConfigVersion
           ,@CanDatabaseVersion
           ,@BusArchitecture
           ,@SupplierRegionId
           ,@ProvisioningRegionId
           ,@PlantShipDate
           ,@IccId
           ,@Msisdn
           ,@ProtoVersionId
           ,@MfgFacilityId
           ,@ProcessorType
           ,@PsKeyHash
           ,@PsKey
           ,@IsActive
           ,@CreatedBy
           ,@CreatedDate
           ,@LastModifiedBy
           ,@LastModifiedDate)

End
ELSE
begin
	Update  Tcu
	set
            Imei= @Imei
           ,SimImsi = @SimImsi
           ,TcuAssemblyPartNumber= @TcuAssemblyPartNumber
           ,TcuHardwarePartNumber   = @TcuHardwarePartNumber
           ,FirmwareVersion   = @FirmwareVersion
           ,FirmwareVersionTeseo2   = @FirmwareVersionTeseo2
           ,FirmwareVersionHe920   = @FirmwareVersionHe920
           ,StrategyPartNumber   = @StrategyPartNumber
           ,ConfigPartNumber   = @ConfigPartNumber
           ,GlobalConfigVersion   = @GlobalConfigVersion
           ,CanDatabaseVersion   = @CanDatabaseVersion
           ,BusArchitecture   = @BusArchitecture
           ,SupplierRegionId   = @SupplierRegionId
           ,ProvisioningRegionId   = @ProvisioningRegionId
           ,PlantShipDate   = @PlantShipDate
           ,IccId   = @IccId
           ,Msisdn   = @Msisdn
           ,ProtoVersionId   = @ProtoVersionId
           ,MfgFacilityId   = @MfgFacilityId
           ,ProcessorType   = @ProcessorType
           ,PsKeyHash   = @PsKeyHash
           ,PsKey   = @PsKey
           ,IsActive   = @IsActive
           ,LastModifiedBy   = @LastModifiedBy
           ,LastModifiedDate   = @LastModifiedDate

		where 
		Esn = @Esn
end
End
GO
/****** Object:  StoredProcedure [dbo].[RemoveTcu]    Script Date: 2/22/2016 4:38:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create Procedure [dbo].[RemoveTcu]
(
@Esn nvarchar(8)
)
As
Begin

If exists(Select Esn from Tcu where Esn = @Esn)
Begin
Delete from Tcu where Esn = @Esn
End

End
GO
/****** Object:  StoredProcedure [dbo].[UpdatetcuIsActive]    Script Date: 2/22/2016 4:38:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Procedure [dbo].[UpdatetcuIsActive]
@Esn nvarchar(8),
@LastModifiedDate datetime = NULL
AS
Begin
update tcu 
set 
IsActive = 1,
LastModifiedDate = @LastModifiedDate
where Esn = @Esn
End

GO
/****** Object:  StoredProcedure [dbo].[usp_LogCommand]    Script Date: 2/22/2016 4:38:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_LogCommand]
	-- Add the parameters for the stored procedure here
	@Vin varchar(35), 
	@Esn varchar(20), 
	@CommandType varchar(50), 
	@MessageId varchar(50), 
	@CorrelationId varchar(50), 
	@PublishedOn datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	Insert INTO TCUCommand values (@Esn, @Vin, @CommandType, @MessageId, @CorrelationId, @PublishedOn, getdate())
END

GO
/****** Object:  StoredProcedure [dbo].[usp_ProcessConnectionStatusAlert]    Script Date: 2/22/2016 4:38:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[usp_ProcessConnectionStatusAlert] 
	  @Vin NVARCHAR(35), 
	  @Esn NVARCHAR(20),
	  @ProvisionedDate NVARCHAR(30),
	  @AuthorizationStatus INTEGER,
	  @ProtoVersion NVARCHAR(30)
AS

DECLARE @retry INT;
SET @retry = 3;

WHILE (@retry > 0)
BEGIN

	-- Check if TCU exists in tcu table
	IF NOT EXISTS (SELECT * FROM Tcu WHERE Esn = @Esn)
	BEGIN
		SELECT -1;
		RETURN;
	END

	BEGIN TRY
		BEGIN TRANSACTION;
		DECLARE @ProtoVersionId AS INT = -1

		UPDATE TcuVehicle SET ActiveTo = getutcdate(), LastmodifiedBy = 'Sdn', LastModifiedDate = getutcdate() WHERE (Esn = @Esn AND Vin != @Vin) OR (Vin = @Vin and Esn != @Esn) AND ActiveTo >= GETUTCDATE()

		IF NOT EXISTS (SELECT * FROM Tcu WHERE Esn = @Esn)
		BEGIN
			SELECT -1;
			RETURN;
		END

		IF EXISTS (SELECT * FROM TcuVehicle WHERE Esn = @Esn And Vin = @Vin AND ActiveTo > GETUTCDATE())
		BEGIN
			Update TcuVehicle 
			SET TcuAuthStatusId = @AuthorizationStatus,
				Lastmodifiedby = 'Sdn', 
				LastModifiedDate = getutcdate()
			WHERE Esn = @Esn And Vin = @Vin AND ActiveTo > getutcdate()
	
		END

		ELSE
		BEGIN
			IF NOT EXISTS(SELECT 1 FROM Vehicle WHERE Vin = @Vin)
			BEGIN
				INSERT INTO Vehicle(Vin) VALUES(@Vin)
			END
			
			INSERT INTO TcuVehicle
			VALUES 
			(
				@Esn, 
				@Vin, 
				'9999-12-31 23:59:59.997', 
				@ProvisionedDate, 
				@AuthorizationStatus, 
				'Sdn', 
				getutcdate(), 
				'Sdn', 
				Getutcdate()
			)
		END

		SELECT @ProtoVersionId = protoversionId FROM ProtoVersion WHERE SdnProtoVersion = @ProtoVersion

		IF @ProtoVersionId <> -1
		BEGIN
				UPDATE Tcu 
				SET protoversionId = @ProtoVersionId,
					Lastmodifiedby = 'Sdn', 
					LastModifiedDate = getutcdate() 
				WHERE Esn = @Esn
		End

		COMMIT TRANSACTION;
		SET @retry = 0;
		SELECT 1
	END TRY

	BEGIN CATCH
		-- Check error number.
		-- If deadlock victim error, then reduce retry count for next update retry. 
		-- If some other error occurred, then exit retry WHILE loop.
		IF (ERROR_NUMBER() = 1205)
		BEGIN
			SET @retry = @retry - 1;
		END
		ELSE
		BEGIN
			SET @retry = -1;
			SELECT -1;
			RETURN;
		END
  
		IF XACT_STATE() <> 0
		BEGIN
			ROLLBACK TRANSACTION;
		END
	END CATCH
END; -- End WHILE loop.




GO
/****** Object:  StoredProcedure [dbo].[usp_ProcessTcuMessage]    Script Date: 2/22/2016 4:38:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[usp_ProcessTcuMessage] 
	  @Vin NVARCHAR(35), 
	  @Esn NVARCHAR(20),
	  @MessageType NVARCHAR(50),
	  @MessageSubType NVARCHAR(50),
	  @MessageId NVARCHAR(50),
	  @CorrelationId NVARCHAR(50)
AS

DECLARE @retry INT;
SET @retry = 3;
DECLARE @TcuVehicleId bigint;

WHILE (@retry > 0)
BEGIN
	BEGIN TRY
		BEGIN TRANSACTION;

		-- based on esn and vin, get tcuvehicle id
		SELECT @TcuVehicleId = TcuVehicleId FROM TcuVehicle t WHERE t.Vin = @Vin AND t.Esn = @Esn

		-- insert tcu message
		INSERT INTO TcuMessage
			VALUES 
			(
				@TcuVehicleId,
				@MessageType,
				@MessageSubType,
				Getutcdate(),
				@MessageId,
				@CorrelationId
			)

		COMMIT TRANSACTION;
		SET @retry = 0;
		SELECT 1
	END TRY

	BEGIN CATCH
		-- Check error number.
		-- If deadlock victim error, then reduce retry count for next update retry. 
		-- If some other error occurred, then exit retry WHILE loop.
		IF (ERROR_NUMBER() = 1205)
		BEGIN
			SET @retry = @retry - 1;
		END
		ELSE
		BEGIN
			SET @retry = -1;
			SELECT -1;
			RETURN;
		END
  
		IF XACT_STATE() <> 0
		BEGIN
			ROLLBACK TRANSACTION;
		END
	END CATCH
	
END; -- End WHILE loop.






GO
