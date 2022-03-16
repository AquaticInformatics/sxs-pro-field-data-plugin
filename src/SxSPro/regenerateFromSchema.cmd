rem Regenerate the SxS XML serialization classes from the schema.
rem Assumes that the Microsoft Visual Studio 201x Tool XSD.EXE is accessible on the PATH

xsd Schema\SxSDischargeSummary.xsd /out:Schema /namespace:SxSPro.Schema /classes
