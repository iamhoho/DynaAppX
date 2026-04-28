import fs from 'fs';
import path from 'path';
import { execSync } from 'child_process';
import { fileURLToPath } from 'url';
import crypto from 'crypto';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const configPath = path.join(__dirname, 'solution.config.json');
const distDir = path.join(__dirname, 'dist');
const outputDir = path.join(__dirname, 'DynaAppX_Solution');
const webResourcesDir = path.join(outputDir, 'WebResources');
const zipFileName = 'DynaAppX.zip';

// Load config
const config = JSON.parse(fs.readFileSync(configPath, 'utf8'));

// Ensure output directories exist
function ensureDir(dir) {
  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }
}

// Generate a deterministic hash for filename based on content
function generateFileHash(content) {
  return crypto.createHash('md5').update(content).digest('hex').substring(0, 12);
}

// Get file extension type for WebResourceType
function getWebResourceType(ext) {
  const types = {
    '.html': 1,
    '.htm': 1,
    '.css': 2,
    '.js': 3,
    '.png': 5,
    '.jpg': 5,
    '.jpeg': 5,
    '.gif': 5,
    '.ico': 5,
    '.svg': 9,
    '.woff': 10,
    '.woff2': 10,
    '.ttf': 10,
    '.eot': 11,
    '.json': 12,
  };
  return types[ext] || 3;
}

// Normalize path for webresource name (remove leading slash)
function normalizeWebResourcePath(filePath) {
  return filePath.replace(/\\/g, '/').replace(/^dist\//, 'dax_/');
}

// Get or generate GUID for a web resource
function getGuidForPath(webResourcePath) {
  if (config.webResources && config.webResources[webResourcePath]) {
    return config.webResources[webResourcePath];
  }
  // Generate new GUID
  const guid = crypto.randomUUID().toLowerCase();
  if (config.webResources) {
    config.webResources[webResourcePath] = guid;
  }
  return guid;
}

// Build CRM filename: prefix_pathHashGuid without dashes
function buildCrmFileName(webResourcePath, ext, guid) {
  const prefix = config.publisher.prefix;
  // Remove the dax_/ prefix before cleaning
  const pathWithoutPrefix = webResourcePath.replace(/^dax_\//, '');
  const cleanPath = pathWithoutPrefix.replace(/[^\w]/g, '').substring(0, 30);
  const hash = generateFileHash(webResourcePath);
  const guidClean = guid.replace(/-/g, '').toUpperCase();
  return `${prefix}_${cleanPath}${hash}${ext.replace('.', '')}${guidClean}`;
}

// Generate solution.xml
function generateSolutionXml() {
  return `<?xml version="1.0" encoding="utf-8"?>
<ImportExportXml version="9.0.6.9" SolutionPackageVersion="9.0" languagecode="2052" generatedBy="OnPremise" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <SolutionManifest>
    <UniqueName>${config.solutionName || 'DynaAppX'}</UniqueName>
    <LocalizedNames>
      <LocalizedName description="${config.solutionName || 'DynaAppX'}" languagecode="2052" />
    </LocalizedNames>
    <Descriptions>
      <Description description="${config.description || 'Tools for Dynamics CRM/365/Power Apps users.'}" languagecode="2052" />
    </Descriptions>
    <Version>${config.version}</Version>
    <Managed>${config.managed ? '1' : '0'}</Managed>
    <ConfigurationPage>${Object.keys(config.webResources).find(k => k.includes('index.html')) || 'dax_/index.html'}</ConfigurationPage>
    <Publisher>
      <UniqueName>${config.publisher.uniqueName}</UniqueName>
      <LocalizedNames>
        <LocalizedName description="${config.publisher.displayName}" languagecode="2052" />
      </LocalizedNames>
      <Descriptions />
      <EMailAddress xsi:nil="true">${config.publisher.email || ''}</EMailAddress>
      <SupportingWebsiteUrl xsi:nil="true">${config.publisher.website || ''}</SupportingWebsiteUrl>
      <CustomizationPrefix>${config.publisher.prefix}</CustomizationPrefix>
      <CustomizationOptionValuePrefix>${config.publisher.optionValuePrefix}</CustomizationOptionValuePrefix>
      <Addresses>
        <Address>
          <AddressNumber>1</AddressNumber>
          <AddressTypeCode>1</AddressTypeCode>
          <City xsi:nil="true"></City>
          <County xsi:nil="true"></County>
          <Country xsi:nil="true"></Country>
          <Fax xsi:nil="true"></Fax>
          <FreightTermsCode xsi:nil="true"></FreightTermsCode>
          <ImportSequenceNumber xsi:nil="true"></ImportSequenceNumber>
          <Latitude xsi:nil="true"></Latitude>
          <Line1 xsi:nil="true"></Line1>
          <Line2 xsi:nil="true"></Line2>
          <Line3 xsi:nil="true"></Line3>
          <Longitude xsi:nil="true"></Longitude>
          <Name xsi:nil="true"></Name>
          <PostalCode xsi:nil="true"></PostalCode>
          <PostOfficeBox xsi:nil="true"></PostOfficeBox>
          <PrimaryContactName xsi:nil="true"></PrimaryContactName>
          <ShippingMethodCode>1</ShippingMethodCode>
          <StateOrProvince xsi:nil="true"></StateOrProvince>
          <Telephone1 xsi:nil="true"></Telephone1>
          <Telephone2 xsi:nil="true"></Telephone2>
          <Telephone3 xsi:nil="true"></Telephone3>
          <TimeZoneRuleVersionNumber xsi:nil="true"></TimeZoneRuleVersionNumber>
          <UPSZone xsi:nil="true"></UPSZone>
          <UTCOffset xsi:nil="true"></UTCOffset>
          <UTCConversionTimeZoneCode xsi:nil="true"></UTCConversionTimeZoneCode>
        </Address>
      </Addresses>
    </Publisher>
    <IsUpgrade>false</IsUpgrade>
  </SolutionManifest>
</ImportExportXml>`;
}

// Generate [Content_Types].xml
function generateContentTypes() {
  return `<?xml version="1.0" encoding="utf-8"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="xml" ContentType="application/xml" />
  <Default Extension="htm" ContentType="text/html" />
  <Default Extension="html" ContentType="text/html" />
  <Default Extension="css" ContentType="text/css" />
  <Default Extension="js" ContentType="application/javascript" />
  <Default Extension="png" ContentType="image/png" />
  <Default Extension="jpg" ContentType="image/jpeg" />
  <Default Extension="jpeg" ContentType="image/jpeg" />
  <Default Extension="gif" ContentType="image/gif" />
  <Default Extension="ico" ContentType="image/x-icon" />
  <Default Extension="svg" ContentType="image/svg+xml" />
  <Default Extension="json" ContentType="application/json" />
</Types>`;
}

// Generate WebResources XML section
function generateWebResourcesXml() {
  let xml = '  <WebResources>\n';
  for (const [webResourcePath, guid] of Object.entries(config.webResources)) {
    const ext = path.extname(webResourcePath);
    const crmFileName = buildCrmFileName(webResourcePath, ext, guid);
    const displayName = path.basename(webResourcePath);

    xml += `    <WebResource>
      <WebResourceId>{${guid.toUpperCase()}}</WebResourceId>
      <Name>${webResourcePath}</Name>
      <DisplayName>${displayName}</DisplayName>
      <WebResourceType>${getWebResourceType(ext)}</WebResourceType>
      <IntroducedVersion>${config.version}</IntroducedVersion>
      <IsEnabledForMobileClient>0</IsEnabledForMobileClient>
      <IsAvailableForMobileOffline>0</IsAvailableForMobileOffline>
      <DependencyXml>&lt;Dependencies&gt;&lt;Dependency componentType="WebResource"/&gt;&lt;/Dependencies&gt;</DependencyXml>
      <IsCustomizable>1</IsCustomizable>
      <CanBeDeleted>1</CanBeDeleted>
      <IsHidden>0</IsHidden>
      <FileName>/WebResources/${crmFileName}</FileName>
    </WebResource>\n`;
  }
  xml += '  </WebResources>';
  return xml;
}

// Get Ribbon XML from RibbonDiffXml.xml file
function getRibbonXml() {
  const ribbonPath = path.join(__dirname, '..', 'RibbonDiffXml.xml');
  if (fs.existsSync(ribbonPath)) {
    return fs.readFileSync(ribbonPath, 'utf8');
  }
  // Fallback to default ribbon
  return `<RibbonDiffXml>
    <CustomActions>
      <CustomAction Id="${config.ribbon.buttonId}.CustomAction" Location="${config.ribbon.ribbonLocation}" Sequence="${config.ribbon.ribbonSequence}">
        <CommandUIDefinition>
          <Button Alt="$LocLabels:${config.ribbon.buttonId}.Alt" Command="${config.ribbon.buttonId}.Command" Id="${config.ribbon.buttonId}" Image16by16="$webresource:${config.publisher.prefix}_/images/DynaAppx.png" LabelText="$LocLabels:${config.ribbon.buttonId}.LabelText" Sequence="${config.ribbon.ribbonSequence}" TemplateAlias="isv" ToolTipTitle="$LocLabels:${config.ribbon.buttonId}.ToolTipTitle" ToolTipDescription="$LocLabels:${config.ribbon.buttonId}.ToolTipDescription"/>
        </CommandUIDefinition>
      </CustomAction>
    </CustomActions>
    <Templates>
      <RibbonTemplates Id="Mscrm.Templates"/>
    </Templates>
    <CommandDefinitions>
      <CommandDefinition Id="${config.ribbon.buttonId}.Command">
        <EnableRules/>
        <DisplayRules/>
        <Actions>
          <Url Address="$webresource:${config.publisher.prefix}_/index.html" WinMode="0"/>
        </Actions>
      </CommandDefinition>
    </CommandDefinitions>
    <RuleDefinitions>
      <TabDisplayRules/>
      <DisplayRules/>
      <EnableRules/>
    </RuleDefinitions>
    <LocLabels>
      <LocLabel Id="${config.ribbon.buttonId}.Alt">
        <Titles>
          <Title description="${config.ribbon.buttonAlt}" languagecode="1033"/>
          <Title description="${config.ribbon.buttonAlt}" languagecode="2052"/>
        </Titles>
      </LocLabel>
      <LocLabel Id="${config.ribbon.buttonId}.LabelText">
        <Titles>
          <Title description="${config.ribbon.buttonLabel}" languagecode="2052"/>
          <Title description="${config.ribbon.buttonLabel}" languagecode="1033"/>
        </Titles>
      </LocLabel>
      <LocLabel Id="${config.ribbon.buttonId}.ToolTipDescription">
        <Titles>
          <Title description="${config.ribbon.buttonTooltip}" languagecode="1033"/>
          <Title description="${config.ribbon.buttonTooltip}" languagecode="2052"/>
        </Titles>
      </LocLabel>
      <LocLabel Id="${config.ribbon.buttonId}.ToolTipTitle">
        <Titles>
          <Title description="${config.ribbon.buttonLabel}" languagecode="1033"/>
          <Title description="${config.ribbon.buttonLabel}" languagecode="2052"/>
        </Titles>
      </LocLabel>
    </LocLabels>
  </RibbonDiffXml>`;
}

// Generate customizations.xml
function generateCustomizationsXml() {
  const ribbonXml = getRibbonXml();

  return `<?xml version="1.0" encoding="utf-8"?>
<ImportExportXml xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <Entities></Entities>
  <Roles></Roles>
  <Workflows></Workflows>
  <FieldSecurityProfiles></FieldSecurityProfiles>
  <Templates />
  ${ribbonXml}
  <EntityMaps />
  <EntityRelationships />
  <OrganizationSettings />
  <optionsets />
  ${generateWebResourcesXml()}
</ImportExportXml>`;
}

// Build the solution
async function build() {
  console.log('Building DynaAppX CRM Solution...\n');

  // Clean previous output
  if (fs.existsSync(outputDir)) {
    fs.rmSync(outputDir, { recursive: true });
  }
  ensureDir(outputDir);
  ensureDir(webResourcesDir);

  // Build Vue project
  console.log('1. Building Vue project...');
  try {
    execSync('npm run build', { cwd: __dirname, stdio: 'inherit' });
  } catch (e) {
    console.error('Vue build failed!');
    process.exit(1);
  }

  // Copy additional static files from src/assets to dist
  console.log('\n2. Copying static assets...');
  const assetsDir = path.join(__dirname, 'src', 'assets');
  const metadataBrowserDir = path.join(assetsDir, 'MetadataBrowser');
  if (fs.existsSync(metadataBrowserDir)) {
    const destDir = path.join(distDir, 'metadatabrowser');
    ensureDir(destDir);
    fs.readdirSync(metadataBrowserDir).forEach(file => {
      fs.copyFileSync(path.join(metadataBrowserDir, file), path.join(destDir, file));
      console.log(`   metadatabrowser/${file}`);
    });
  }

  // Copy DynaAppx.png for ribbon
  const dynaAppxPng = path.join(__dirname, '..', 'DynaAppx.png');
  const imagesDir = path.join(distDir, 'images');
  if (fs.existsSync(dynaAppxPng)) {
    ensureDir(imagesDir);
    fs.copyFileSync(dynaAppxPng, path.join(imagesDir, 'DynaAppx.png'));
    console.log(`   images/DynaAppx.png`);
  }

  // Update config with dist files
  console.log('\n3. Processing dist files...');
  const distFiles = getAllFiles(distDir);
  const webResources = {};

  for (const file of distFiles) {
    const relativePath = path.relative(distDir, file);
    const webResourcePath = 'dax_/' + relativePath.replace(/\\/g, '/');
    const guid = getGuidForPath(webResourcePath);
    webResources[webResourcePath] = guid;

    const ext = path.extname(file);
    const crmFileName = buildCrmFileName(webResourcePath, ext, guid);

    // Copy file with CRM filename
    fs.copyFileSync(file, path.join(webResourcesDir, crmFileName));
    console.log(`   ${relativePath} -> ${crmFileName}`);
  }

  // Update config
  config.webResources = webResources;

  // Generate XML files
  console.log('\n4. Generating solution XML files...');
  fs.writeFileSync(path.join(outputDir, 'solution.xml'), generateSolutionXml());
  fs.writeFileSync(path.join(outputDir, 'customizations.xml'), generateCustomizationsXml());
  fs.writeFileSync(path.join(outputDir, '[Content_Types].xml'), generateContentTypes());
  console.log('   solution.xml');
  console.log('   customizations.xml');
  console.log('   [Content_Types].xml');

  // Create zip
  console.log('\n5. Creating solution package...');
  try {
    execSync(`cd "${outputDir}" && zip -r "${zipFileName}" .`, { stdio: 'inherit' });
  } catch (e) {
    console.error('Zip failed!');
    process.exit(1);
  }

  const zipPath = path.join(outputDir, zipFileName);
  const stats = fs.statSync(zipPath);
  console.log(`\n✓ Solution package created: ${zipPath} (${(stats.size / 1024).toFixed(1)} KB)`);

  // Save updated config
  fs.writeFileSync(configPath, JSON.stringify(config, null, 2));
  console.log('✓ Config updated with new GUIDs');
}

function getAllFiles(dir) {
  const files = [];
  const items = fs.readdirSync(dir, { withFileTypes: true });
  for (const item of items) {
    const fullPath = path.join(dir, item.name);
    if (item.isDirectory()) {
      files.push(...getAllFiles(fullPath));
    } else {
      files.push(fullPath);
    }
  }
  return files;
}

build().catch(console.error);