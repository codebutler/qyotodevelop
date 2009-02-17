# Begin MonoDevelop-maintained variables

FILES =  \
	AssemblyInfo.cs \
	Commands.cs \
	EventyList.cs \
	ProjectFolderNodeBuilder.cs \
	ProjectNodeBuilder.cs \
	QyotoCodeGenerator.cs \
	QyotoCommandHandler.cs \
	QyotoDesignInfo.cs \
	QyotoForm.cs \
	QyotoFormFileDescriptionTemplate.cs \
	QyotoFormNodeBuilder.cs \
	QyotoFormPropertyDescriptor.cs \
	QyotoFormPropertyProvider.cs \
	QyotoFormsFolderNodeBuilder.cs \
	QyotoProjectFeatures.cs \
	QyotoProjectServiceExtension.cs \
	QyotoStartupCommand.cs \
	ReferenceManager.cs \
	Util.cs

RESOURCES =  \
	Resources/Dialog.xft.xml \
	Resources/EmptyCSharpQyotoProject.xpt.xml \
	Resources/MainWindow.xft.xml \
	Resources/QyotoDevelop.addin.xml \
	Resources/Widget.xft.xml 

REFERENCES =  \
	$(GTK_SHARP_20_LIBS) \
	$(MONODEVELOP_CORE_ADDINS_LIBS) \
	$(MONODEVELOP_LIBS) \
	System \
	System.Xml

PROJECT_REFERENCES = 

DLL_REFERENCES =  

# End MonoDevelop-maintained variables

ASSEMBLY = $(BUILD_DIR)/QyotoDevelop.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb

QYOTODEVELOP_CORE_PC = $(BUILD_DIR)/qyotodevelop.pc

LINUX_PKGCONFIG = \
	$(QYOTODEVELOP_CORE_PC)

EXTRA_DIST = $(QYOTODEVELOP_CORE_PC)

all: $(ASSEMBLY) $(ASSEMBLY_MDB) $(LINUX_PKGCONFIG)

include $(top_srcdir)/Makefile.include

$(eval $(call emit-deploy-wrapper,QYOTODEVELOP_CORE_PC,qyotodevelop.pc))

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(DLL_REFERENCES) $(PROJECT_REFERENCES)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library $(build_sources) $(build_resources_embed) $(build_references_ref)
