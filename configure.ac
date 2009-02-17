dnl Process this file with autoconf to produce a configure script.

AC_PREREQ([2.54])
AC_INIT([QyotoDevelop], [0.1])
AM_INIT_AUTOMAKE([foreign])

dnl pkg-config
AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
        AC_MSG_ERROR([You need to install pkg-config])
fi

AC_PROG_INSTALL

dnl Check for C# compiler
AC_PATH_PROG(MCS, gmcs, no)
if test "x$MCS" = "xno"; then
        AC_MSG_ERROR([gmcs Not found])
fi
MCS_FLAGS="-noconfig -codepage:utf8 -warn:4 -optimize- -debug \"-define:DEBUG\""
AC_SUBST(MCS)
AC_SUBST(MCS_FLAGS)

dnl package checks


PKG_CHECK_MODULES([GTK_SHARP_20], [gtk-sharp-2.0])
AC_SUBST(GTK_SHARP_20_LIBS)

PKG_CHECK_MODULES([MONODEVELOP], [monodevelop])
AC_SUBST(MONODEVELOP_LIBS)

PKG_CHECK_MODULES([MONO_ADDINS], [mono-addins])
AC_SUBST(MONO_ADDINS_LIBS)

PKG_CHECK_MODULES([MONODEVELOP_CORE_ADDINS], [monodevelop-core-addins])
AC_SUBST(MONODEVELOP_CORE_ADDINS_LIBS)

BUILD_DIR="\$(top_builddir)/bin"
AC_SUBST(BUILD_DIR)

AC_CONFIG_FILES([
Makefile
QyotoDevelop/Makefile
])

AC_OUTPUT
