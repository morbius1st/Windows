﻿<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="14.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')" />
  <PropertyGroup>
    <ResolveAssemblyWarnOrErrorOnTargetArchitectureMismatch>
      None
    </ResolveAssemblyWarnOrErrorOnTargetArchitectureMismatch>
    <TargetFrameworkProfile />
  </PropertyGroup>
  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
    <ProjectGuid>{DDC429DE-0FCC-45A7-A152-112E9AB46CC4}</ProjectGuid>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>RevitWindows</RootNamespace>
    <AssemblyName>RevitWindows</AssemblyName>
    <TargetFrameworkVersion>v4.5.2</TargetFrameworkVersion>
    <FileAlignment>512</FileAlignment>
  </PropertyGroup>
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <StartAction>Program</StartAction>
    <StartProgram>$(ProgramW6432)\Autodesk\Revit 2017\Revit.exe</StartProgram>
    <Prefer32Bit>false</Prefer32Bit>
  </PropertyGroup>
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ">
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\Release\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <StartAction>Program</StartAction>
    <StartProgram>$(ProgramW6432)\Autodesk\Revit 2017\Revit.exe</StartProgram>
    <Prefer32Bit>false</Prefer32Bit>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="envdte, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a">
      <EmbedInteropTypes>True</EmbedInteropTypes>
    </Reference>
    <Reference Include="envdte80, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a">
      <EmbedInteropTypes>True</EmbedInteropTypes>
    </Reference>
    <Reference Include="PresentationCore" />
    <Reference Include="PresentationFramework" />
    <Reference Include="RevitAPI">
      <HintPath>$(ProgramW6432)\Autodesk\Revit 2017\RevitAPI.dll</HintPath>
      <Private>False</Private>
    </Reference>
    <Reference Include="RevitAPIUI">
      <HintPath>$(ProgramW6432)\Autodesk\Revit 2017\RevitAPIUI.dll</HintPath>
      <Private>False</Private>
    </Reference>
    <Reference Include="System" />
    <Reference Include="System.Drawing" />
    <Reference Include="System.Windows.Forms" />
    <Reference Include="WindowsBase" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include="Extensions.cs" />
    <Compile Include="MainForm.cs">
      <SubType>Form</SubType>
    </Compile>
    <Compile Include="MainForm.Designer.cs">
      <DependentUpon>MainForm.cs</DependentUpon>
    </Compile>
    <Compile Include="MessageUtilities.cs" />
    <Compile Include="Properties\AssemblyInfo.cs" />
    <Compile Include="Properties\Resources.Designer.cs">
      <AutoGen>True</AutoGen>
      <DesignTime>True</DesignTime>
      <DependentUpon>Resources.resx</DependentUpon>
    </Compile>
    <Compile Include="ResourceSettings.Designer.cs">
      <AutoGen>True</AutoGen>
      <DesignTime>True</DesignTime>
      <DependentUpon>ResourceSettings.resx</DependentUpon>
    </Compile>
    <Compile Include="RevitWindow.cs" />
    <Compile Include="Ribbon.cs" />
    <Compile Include="RibbonUtil.cs" />
    <Compile Include="RwCommands.cs" />
    <Compile Include="Properties\Settings.Designer.cs">
      <AutoGen>True</AutoGen>
      <DesignTimeSharedInput>True</DesignTimeSharedInput>
      <DependentUpon>Settings.settings</DependentUpon>
    </Compile>
    <Compile Include="Settings.cs" />
    <Compile Include="SettingsForm.cs">
      <SubType>Form</SubType>
    </Compile>
    <Compile Include="SettingsForm.Designer.cs">
      <DependentUpon>SettingsForm.cs</DependentUpon>
    </Compile>
    <Compile Include="UserSettings.cs" />
    <Compile Include="UserSettings.Designer.cs">
      <AutoGen>True</AutoGen>
      <DesignTimeSharedInput>True</DesignTimeSharedInput>
      <DependentUpon>UserSettings.settings</DependentUpon>
    </Compile>
    <Compile Include="WindowListingUtilities.cs" />
    <Compile Include="WindowManager.cs" />
    <Compile Include="WindowApiUtilities.cs" />
    <Compile Include="WindowManagerUtilities.cs" />
    <Compile Include="WindowUtilities.cs" />
  </ItemGroup>
  <ItemGroup>
    <EmbeddedResource Include="MainForm.resx">
      <DependentUpon>MainForm.cs</DependentUpon>
    </EmbeddedResource>
    <EmbeddedResource Include="Properties\Resources.resx">
      <Generator>ResXFileCodeGenerator</Generator>
      <LastGenOutput>Resources.Designer.cs</LastGenOutput>
    </EmbeddedResource>
    <EmbeddedResource Include="ResourceSettings.resx">
      <Generator>ResXFileCodeGenerator</Generator>
      <LastGenOutput>ResourceSettings.Designer.cs</LastGenOutput>
    </EmbeddedResource>
    <EmbeddedResource Include="SettingsForm.resx">
      <DependentUpon>SettingsForm.cs</DependentUpon>
    </EmbeddedResource>
  </ItemGroup>
  <ItemGroup>
    <EmbeddedResource Include="Resources\Images\information16.png" />
  </ItemGroup>
  <ItemGroup>
    <EmbeddedResource Include="Resources\Images\information32.png" />
  </ItemGroup>
  <ItemGroup>
    <None Include="app.config" />
    <None Include="Properties\Settings.settings">
      <Generator>SettingsSingleFileGenerator</Generator>
      <LastGenOutput>Settings.Designer.cs</LastGenOutput>
    </None>
    <None Include="UserSettings.settings">
      <Generator>SettingsSingleFileGenerator</Generator>
      <LastGenOutput>UserSettings.Designer.cs</LastGenOutput>
    </None>
  </ItemGroup>
  <Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />
  <PropertyGroup>
    <PostBuildEvent>if exist "$(AppData)\Autodesk\REVIT\Addins\2017" copy "$(SolutionDir)*.addin" "$(AppData)\Autodesk\REVIT\Addins\2017"
if exist "$(AppData)\Autodesk\REVIT\Addins\2017" copy "$(ProjectDir)$(OutputPath)*.dll" "$(AppData)\Autodesk\REVIT\Addins\2017"</PostBuildEvent>
  </PropertyGroup>
  <!-- To modify your build process, add your task inside one of the targets below and uncomment it.
       Other similar extension points exist, see Microsoft.Common.targets.
  <Target Name="BeforeBuild">
  </Target>
  <Target Name="AfterBuild">
  </Target>
  -->
  <Target Name="AfterClean">
    <Delete Files="$(AppData)\Autodesk\REVIT\Addins\2017\Windows.addin" />
    <Delete Files="$(AppData)\Autodesk\REVIT\Addins\2017\Windows.dll" />
  </Target>
</Project>