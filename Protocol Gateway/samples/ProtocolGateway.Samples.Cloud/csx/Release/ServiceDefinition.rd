<?xml version="1.0" encoding="utf-8"?>
<serviceModel xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" name="Contoso.ProtocolGateway" generation="1" functional="0" release="0" Id="f2f2fd5e-173f-42b8-84ee-07d825111da5" dslVersion="1.2.0.0" xmlns="http://schemas.microsoft.com/dsltools/RDSM">
  <groups>
    <group name="Contoso.ProtocolGatewayGroup" generation="1" functional="0" release="0">
      <componentports>
        <inPort name="ProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" protocol="tcp">
          <inToChannel>
            <lBChannelMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/LB:ProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" />
          </inToChannel>
        </inPort>
        <inPort name="ProtocolGateway.Samples.Cloud.Host:MQTT" protocol="tcp">
          <inToChannel>
            <lBChannelMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/LB:ProtocolGateway.Samples.Cloud.Host:MQTT" />
          </inToChannel>
        </inPort>
        <inPort name="ProtocolGateway.Samples.Cloud.Host:MQTTS" protocol="tcp">
          <inToChannel>
            <lBChannelMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/LB:ProtocolGateway.Samples.Cloud.Host:MQTTS" />
          </inToChannel>
        </inPort>
      </componentports>
      <settings>
        <aCS name="Certificate|ProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapCertificate|ProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
          </maps>
        </aCS>
        <aCS name="Certificate|ProtocolGateway.Samples.Cloud.Host:TlsCertificate" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapCertificate|ProtocolGateway.Samples.Cloud.Host:TlsCertificate" />
          </maps>
        </aCS>
        <aCS name="ProtocolGateway.Samples.Cloud.Host:BlobSessionStatePersistenceProvider.StorageConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapProtocolGateway.Samples.Cloud.Host:BlobSessionStatePersistenceProvider.StorageConnectionString" />
          </maps>
        </aCS>
        <aCS name="ProtocolGateway.Samples.Cloud.Host:BlobSessionStatePersistenceProvider.StorageContainerName" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapProtocolGateway.Samples.Cloud.Host:BlobSessionStatePersistenceProvider.StorageContainerName" />
          </maps>
        </aCS>
        <aCS name="ProtocolGateway.Samples.Cloud.Host:ConnectArrivalTimeout" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapProtocolGateway.Samples.Cloud.Host:ConnectArrivalTimeout" />
          </maps>
        </aCS>
        <aCS name="ProtocolGateway.Samples.Cloud.Host:DefaultPublishToClientQoS" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapProtocolGateway.Samples.Cloud.Host:DefaultPublishToClientQoS" />
          </maps>
        </aCS>
        <aCS name="ProtocolGateway.Samples.Cloud.Host:DeviceReceiveAckTimeout" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapProtocolGateway.Samples.Cloud.Host:DeviceReceiveAckTimeout" />
          </maps>
        </aCS>
        <aCS name="ProtocolGateway.Samples.Cloud.Host:DupPropertyName" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapProtocolGateway.Samples.Cloud.Host:DupPropertyName" />
          </maps>
        </aCS>
        <aCS name="ProtocolGateway.Samples.Cloud.Host:IoTHubConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapProtocolGateway.Samples.Cloud.Host:IoTHubConnectionString" />
          </maps>
        </aCS>
        <aCS name="ProtocolGateway.Samples.Cloud.Host:IsNonTLSEnabled" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapProtocolGateway.Samples.Cloud.Host:IsNonTLSEnabled" />
          </maps>
        </aCS>
        <aCS name="ProtocolGateway.Samples.Cloud.Host:MaxInboundMessageSize" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapProtocolGateway.Samples.Cloud.Host:MaxInboundMessageSize" />
          </maps>
        </aCS>
        <aCS name="ProtocolGateway.Samples.Cloud.Host:MaxKeepAliveTimeout" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapProtocolGateway.Samples.Cloud.Host:MaxKeepAliveTimeout" />
          </maps>
        </aCS>
        <aCS name="ProtocolGateway.Samples.Cloud.Host:MaxOutboundRetransmissionCount" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapProtocolGateway.Samples.Cloud.Host:MaxOutboundRetransmissionCount" />
          </maps>
        </aCS>
        <aCS name="ProtocolGateway.Samples.Cloud.Host:MaxPendingInboundMessages" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapProtocolGateway.Samples.Cloud.Host:MaxPendingInboundMessages" />
          </maps>
        </aCS>
        <aCS name="ProtocolGateway.Samples.Cloud.Host:MaxPendingOutboundMessages" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapProtocolGateway.Samples.Cloud.Host:MaxPendingOutboundMessages" />
          </maps>
        </aCS>
        <aCS name="ProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="ProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" />
          </maps>
        </aCS>
        <aCS name="ProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" />
          </maps>
        </aCS>
        <aCS name="ProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" />
          </maps>
        </aCS>
        <aCS name="ProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" />
          </maps>
        </aCS>
        <aCS name="ProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" />
          </maps>
        </aCS>
        <aCS name="ProtocolGateway.Samples.Cloud.Host:QoSPropertyName" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapProtocolGateway.Samples.Cloud.Host:QoSPropertyName" />
          </maps>
        </aCS>
        <aCS name="ProtocolGateway.Samples.Cloud.Host:RetainPropertyName" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapProtocolGateway.Samples.Cloud.Host:RetainPropertyName" />
          </maps>
        </aCS>
        <aCS name="ProtocolGateway.Samples.Cloud.Host:TableQos2StatePersistenceProvider.StorageConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapProtocolGateway.Samples.Cloud.Host:TableQos2StatePersistenceProvider.StorageConnectionString" />
          </maps>
        </aCS>
        <aCS name="ProtocolGateway.Samples.Cloud.Host:TableQos2StatePersistenceProvider.StorageTableName" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapProtocolGateway.Samples.Cloud.Host:TableQos2StatePersistenceProvider.StorageTableName" />
          </maps>
        </aCS>
        <aCS name="ProtocolGateway.Samples.Cloud.Host:TlsCertificateThumbprint" defaultValue="">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapProtocolGateway.Samples.Cloud.Host:TlsCertificateThumbprint" />
          </maps>
        </aCS>
        <aCS name="ProtocolGateway.Samples.Cloud.HostInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/MapProtocolGateway.Samples.Cloud.HostInstances" />
          </maps>
        </aCS>
      </settings>
      <channels>
        <lBChannel name="LB:ProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput">
          <toPorts>
            <inPortMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" />
          </toPorts>
        </lBChannel>
        <lBChannel name="LB:ProtocolGateway.Samples.Cloud.Host:MQTT">
          <toPorts>
            <inPortMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/MQTT" />
          </toPorts>
        </lBChannel>
        <lBChannel name="LB:ProtocolGateway.Samples.Cloud.Host:MQTTS">
          <toPorts>
            <inPortMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/MQTTS" />
          </toPorts>
        </lBChannel>
        <sFSwitchChannel name="SW:ProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp">
          <toPorts>
            <inPortMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" />
          </toPorts>
        </sFSwitchChannel>
      </channels>
      <maps>
        <map name="MapCertificate|ProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" kind="Identity">
          <certificate>
            <certificateMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
          </certificate>
        </map>
        <map name="MapCertificate|ProtocolGateway.Samples.Cloud.Host:TlsCertificate" kind="Identity">
          <certificate>
            <certificateMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/TlsCertificate" />
          </certificate>
        </map>
        <map name="MapProtocolGateway.Samples.Cloud.Host:BlobSessionStatePersistenceProvider.StorageConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/BlobSessionStatePersistenceProvider.StorageConnectionString" />
          </setting>
        </map>
        <map name="MapProtocolGateway.Samples.Cloud.Host:BlobSessionStatePersistenceProvider.StorageContainerName" kind="Identity">
          <setting>
            <aCSMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/BlobSessionStatePersistenceProvider.StorageContainerName" />
          </setting>
        </map>
        <map name="MapProtocolGateway.Samples.Cloud.Host:ConnectArrivalTimeout" kind="Identity">
          <setting>
            <aCSMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/ConnectArrivalTimeout" />
          </setting>
        </map>
        <map name="MapProtocolGateway.Samples.Cloud.Host:DefaultPublishToClientQoS" kind="Identity">
          <setting>
            <aCSMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/DefaultPublishToClientQoS" />
          </setting>
        </map>
        <map name="MapProtocolGateway.Samples.Cloud.Host:DeviceReceiveAckTimeout" kind="Identity">
          <setting>
            <aCSMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/DeviceReceiveAckTimeout" />
          </setting>
        </map>
        <map name="MapProtocolGateway.Samples.Cloud.Host:DupPropertyName" kind="Identity">
          <setting>
            <aCSMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/DupPropertyName" />
          </setting>
        </map>
        <map name="MapProtocolGateway.Samples.Cloud.Host:IoTHubConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/IoTHubConnectionString" />
          </setting>
        </map>
        <map name="MapProtocolGateway.Samples.Cloud.Host:IsNonTLSEnabled" kind="Identity">
          <setting>
            <aCSMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/IsNonTLSEnabled" />
          </setting>
        </map>
        <map name="MapProtocolGateway.Samples.Cloud.Host:MaxInboundMessageSize" kind="Identity">
          <setting>
            <aCSMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/MaxInboundMessageSize" />
          </setting>
        </map>
        <map name="MapProtocolGateway.Samples.Cloud.Host:MaxKeepAliveTimeout" kind="Identity">
          <setting>
            <aCSMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/MaxKeepAliveTimeout" />
          </setting>
        </map>
        <map name="MapProtocolGateway.Samples.Cloud.Host:MaxOutboundRetransmissionCount" kind="Identity">
          <setting>
            <aCSMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/MaxOutboundRetransmissionCount" />
          </setting>
        </map>
        <map name="MapProtocolGateway.Samples.Cloud.Host:MaxPendingInboundMessages" kind="Identity">
          <setting>
            <aCSMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/MaxPendingInboundMessages" />
          </setting>
        </map>
        <map name="MapProtocolGateway.Samples.Cloud.Host:MaxPendingOutboundMessages" kind="Identity">
          <setting>
            <aCSMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/MaxPendingOutboundMessages" />
          </setting>
        </map>
        <map name="MapProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </setting>
        </map>
        <map name="MapProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" kind="Identity">
          <setting>
            <aCSMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" />
          </setting>
        </map>
        <map name="MapProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" kind="Identity">
          <setting>
            <aCSMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" />
          </setting>
        </map>
        <map name="MapProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" kind="Identity">
          <setting>
            <aCSMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" />
          </setting>
        </map>
        <map name="MapProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" kind="Identity">
          <setting>
            <aCSMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" />
          </setting>
        </map>
        <map name="MapProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" kind="Identity">
          <setting>
            <aCSMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" />
          </setting>
        </map>
        <map name="MapProtocolGateway.Samples.Cloud.Host:QoSPropertyName" kind="Identity">
          <setting>
            <aCSMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/QoSPropertyName" />
          </setting>
        </map>
        <map name="MapProtocolGateway.Samples.Cloud.Host:RetainPropertyName" kind="Identity">
          <setting>
            <aCSMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/RetainPropertyName" />
          </setting>
        </map>
        <map name="MapProtocolGateway.Samples.Cloud.Host:TableQos2StatePersistenceProvider.StorageConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/TableQos2StatePersistenceProvider.StorageConnectionString" />
          </setting>
        </map>
        <map name="MapProtocolGateway.Samples.Cloud.Host:TableQos2StatePersistenceProvider.StorageTableName" kind="Identity">
          <setting>
            <aCSMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/TableQos2StatePersistenceProvider.StorageTableName" />
          </setting>
        </map>
        <map name="MapProtocolGateway.Samples.Cloud.Host:TlsCertificateThumbprint" kind="Identity">
          <setting>
            <aCSMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/TlsCertificateThumbprint" />
          </setting>
        </map>
        <map name="MapProtocolGateway.Samples.Cloud.HostInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.HostInstances" />
          </setting>
        </map>
      </maps>
      <components>
        <groupHascomponents>
          <role name="ProtocolGateway.Samples.Cloud.Host" generation="1" functional="0" release="0" software="D:\FCCIotHub\Protocol Gateway\samples\ProtocolGateway.Samples.Cloud\csx\Release\roles\ProtocolGateway.Samples.Cloud.Host" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaWorkerHost.exe " memIndex="-1" hostingEnvironment="consoleroleadmin" hostingEnvironmentVersion="2">
            <componentports>
              <inPort name="Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" protocol="tcp" />
              <inPort name="MQTT" protocol="tcp" portRanges="1883" />
              <inPort name="MQTTS" protocol="tcp" portRanges="8883" />
              <inPort name="Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" protocol="tcp" portRanges="3389" />
              <outPort name="ProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" protocol="tcp">
                <outToChannel>
                  <sFSwitchChannelMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/SW:ProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" />
                </outToChannel>
              </outPort>
            </componentports>
            <settings>
              <aCS name="BlobSessionStatePersistenceProvider.StorageConnectionString" defaultValue="" />
              <aCS name="BlobSessionStatePersistenceProvider.StorageContainerName" defaultValue="" />
              <aCS name="ConnectArrivalTimeout" defaultValue="" />
              <aCS name="DefaultPublishToClientQoS" defaultValue="" />
              <aCS name="DeviceReceiveAckTimeout" defaultValue="" />
              <aCS name="DupPropertyName" defaultValue="" />
              <aCS name="IoTHubConnectionString" defaultValue="" />
              <aCS name="IsNonTLSEnabled" defaultValue="" />
              <aCS name="MaxInboundMessageSize" defaultValue="" />
              <aCS name="MaxKeepAliveTimeout" defaultValue="" />
              <aCS name="MaxOutboundRetransmissionCount" defaultValue="" />
              <aCS name="MaxPendingInboundMessages" defaultValue="" />
              <aCS name="MaxPendingOutboundMessages" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" defaultValue="" />
              <aCS name="QoSPropertyName" defaultValue="" />
              <aCS name="RetainPropertyName" defaultValue="" />
              <aCS name="TableQos2StatePersistenceProvider.StorageConnectionString" defaultValue="" />
              <aCS name="TableQos2StatePersistenceProvider.StorageTableName" defaultValue="" />
              <aCS name="TlsCertificateThumbprint" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;ProtocolGateway.Samples.Cloud.Host&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;ProtocolGateway.Samples.Cloud.Host&quot;&gt;&lt;e name=&quot;Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp&quot; /&gt;&lt;e name=&quot;Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput&quot; /&gt;&lt;e name=&quot;MQTT&quot; /&gt;&lt;e name=&quot;MQTTS&quot; /&gt;&lt;/r&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
            <storedcertificates>
              <storedCertificate name="Stored0Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" certificateStore="My" certificateLocation="System">
                <certificate>
                  <certificateMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
                </certificate>
              </storedCertificate>
              <storedCertificate name="Stored1TlsCertificate" certificateStore="My" certificateLocation="System">
                <certificate>
                  <certificateMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host/TlsCertificate" />
                </certificate>
              </storedCertificate>
            </storedcertificates>
            <certificates>
              <certificate name="Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
              <certificate name="TlsCertificate" />
            </certificates>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.HostInstances" />
            <sCSPolicyUpdateDomainMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.HostUpgradeDomains" />
            <sCSPolicyFaultDomainMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.HostFaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
      </components>
      <sCSPolicy>
        <sCSPolicyUpdateDomain name="ProtocolGateway.Samples.Cloud.HostUpgradeDomains" defaultPolicy="[5,5,5]" />
        <sCSPolicyFaultDomain name="ProtocolGateway.Samples.Cloud.HostFaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyID name="ProtocolGateway.Samples.Cloud.HostInstances" defaultPolicy="[1,1,1]" />
      </sCSPolicy>
    </group>
  </groups>
  <implements>
    <implementation Id="446e4d6f-8ef8-4428-a4e1-069a8ad6d91f" ref="Microsoft.RedDog.Contract\ServiceContract\Contoso.ProtocolGatewayContract@ServiceDefinition">
      <interfacereferences>
        <interfaceReference Id="98ea8786-f251-4365-9343-15fdf79023b3" ref="Microsoft.RedDog.Contract\Interface\ProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput@ServiceDefinition">
          <inPort>
            <inPortMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" />
          </inPort>
        </interfaceReference>
        <interfaceReference Id="eba55ac3-970f-48a3-9c5b-1d2ab6cb09b6" ref="Microsoft.RedDog.Contract\Interface\ProtocolGateway.Samples.Cloud.Host:MQTT@ServiceDefinition">
          <inPort>
            <inPortMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host:MQTT" />
          </inPort>
        </interfaceReference>
        <interfaceReference Id="f7b1266b-d0b2-4189-8429-ae4830f2bede" ref="Microsoft.RedDog.Contract\Interface\ProtocolGateway.Samples.Cloud.Host:MQTTS@ServiceDefinition">
          <inPort>
            <inPortMoniker name="/Contoso.ProtocolGateway/Contoso.ProtocolGatewayGroup/ProtocolGateway.Samples.Cloud.Host:MQTTS" />
          </inPort>
        </interfaceReference>
      </interfacereferences>
    </implementation>
  </implements>
</serviceModel>