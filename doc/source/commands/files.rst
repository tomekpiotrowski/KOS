.. _files:

File I/O
========

For information about where files are kept and how to deal with volumes see the
:ref:`Volumes <volumes>` page in the general topics section of this
documentation.

.. contents::
    :local:
    :depth: 2


Understanding directories
-----------------------------------

kOS, just as real life filesystems, has the ability to group files into
directories. Directories can contain other directories, which can result in
a tree-like structure.

Directories, contrary to files, do not take up space on the volume. That means
you can have as many directories on your volume as you want.

Paths
-----

kOS uses strings of a specific format as a way of describing the location
of files and directories. We will call them path strings or simply - paths.
They will look familiar to users of most real operating systems. On Windows
for example you might have seen something like this::

  C:\Program Files\Some Directory\SomeFile.exe

Linux users are probably more familiar with paths that look like this::

  /home/user/somefile

kOS's paths are quite similar, this is how a full path string might look like::

  0:/lib/launch/base.ks

There are two types of paths in kOS. Absolute paths explicitly state all data
needed to locate an item. Relative paths describe the location of an item
relative to the current directory or current volume.

Absolute paths
~~~~~~~~~~~~~~

Absolute paths have the following format::

  volumeIdOrName:[/directory/subdirectory/...]/filename

The first slash immediately after the colon is optional.

Examples of valid absolute paths::

  0:flight_data/data.json
  secondcpu: // refers to the root directory of a volume
  1:/boot.ks

You can use a special two-dot directory name - `..` - to denote the parent
of a given directory. In the following example the two paths refer to the same
file::

  0:/directory/subdirectory/../file
  0:/directory/file

A path that points to the parent of the root directory of a volume is considered
invalid. Those paths are all invalid::

  0:..
  0:/../..
  0:/directory/../..

Current directory
~~~~~~~~~~~~~~~~~

To facilitate the way you interact with volumes, directories and files kOS
has a concept of current directory. That means you can make a certain directory
a `default` one and kOS will look for files you pass on to kOS commands in that
directory. Let's say for example that you're testing a script located on the
Archive volume in the `launch_scripts` directory. Normally every time you'd like
to do something with it (edit it, run it, copy it etc) you'd have to tell kOS
exactly where that file is.  That could be troublesome, especially when it would
have to be done multiple times.

Instead you can change your current directory using :code:`cd(path)`
(as in `change directory`) command and then refer to all the files and
directories you need by using their relative paths (read more below).

You can always print out the current directory's path like this::

  PRINT PATH().

Remember that you can print the contents of the current directory using the
:code:`LIST` command (which is a shortcut for :code:`LIST FILES`).

Relative paths
~~~~~~~~~~~~~~

Relative paths are the second way you can create paths. Those paths are
transformed by kOS into absolute paths by adding them to the current directory.

Let's say that you've changed your current directory to :code:`0:/scripts`.
If you pass :code:`launch.ks` path to any command kOS will add it to current
directory and create an absolute path this way::

  CD("0:/scripts").
  DELETEPATH("launch.ks"). // this will remove 0:/scripts/launch.ks
  COPYPATH("../launch.ks", ""). // this will copy 0:/launch.ks to 0:/scripts/launch.ks

As you can see above an empty relative path results in a path pointing to the
current directory.

If a relative path starts with :code:`/` kOS will only take the current
directory's volume and add it to the relative path::

  CD("0:/scripts").
  COPYPATH("/launch.ks", "launch_scripts"). // will copy 0:/launch.ks to 0:/scripts/launch_scripts


Paths and bareword arguments
~~~~~~~~~~~~~~~~~~~~~~~~~~~~

.. warning::

  kOS has historically always allowed to skip quotes around file names in certain
  cases. Although it is still possible (explanation below) we recommend against
  it now. kOS 1.0 has introduced directory support and as a result the number of
  cases in which omitting quotes would be fine is less than before. Paths like
  :code:`../file` make things very confusing to the kOS parser because
  kerboscript uses a dot to denote the end of an expression. If you're used
  to skipping quotes you might find that now you will often have to add them to make
  the path understandable to kOS. The only case in which you can reliably omit
  quotes is when you want to use simple, relative paths:
  :code:`RUN script.`, :code:`CD(dir.ext)`.

Any of the commands below which use path arguments follow these rules:

-  A path may be an expression which evaluates to a string.
-  A path may also be an undefined identifier
   which does not match a variable name, in which case the bare word
   name of the identifier will be used as the path. If the
   identifier does match a variable name, then it will be evaluated as
   an expression and the variable's contents will be used as the
   path.
-  A bareword path may contain file extensions with dots, provided
   it does not end in a dot.
-  Bareword filenames containing any characters other than A-Z, 0-9, underscore,
   and the period extension separator ('.'), can only be referred to
   using a string expression (with quotes), and cannot be used as a
   bareword expression (without quotes). This makes it impossible to construct
   valid kOS paths that contain slashes using bareword paths - you will
   need to use quotes.
-  If your filesystem is case-sensitive (Linux and sometimes Mac OSX, or
   even Windows if using some kinds of remote network drives), then
   bareword filenames will only work properly on filenames that are all
   lowercase. If you try to use a file with capital letters in the name
   on these systems, you will only be able to do so by quoting it.

Putting the above rules together, you can create paths in any of
the following ways:

-  COPYPATH(myfilename, "1:"). // This is an example of a bareword filename.
-  COPYPATH("myfilename", "1:"). // This is an example of an EXPRESSION
   filename.
-  COPYPATH(myfilename.ks, "1:"). // This is an example of a bareword
   filename.
-  COPYPATH(myfilename.txt, "1:"). // This is an example of a bareword
   filename.
-  COPYPATH("myfilename.ks", "1:"). // This is an example of an EXPRESSION
   filename
-  SET str TO "myfile" + "name" + ".ks". COPYPATH(str, "1:"). // This is an
   example of an EXPRESSION filename


Other data types as paths
~~~~~~~~~~~~~~~~~~~~~~~~~

Whenever kOS expects a path string as an argument you can actually pass
one of the following data types instead:

- :struct:`Path`
- :struct:`Volume` - will use volume's root path
- :struct:`VolumeFile` - will use file's path
- :struct:`VolumeDirectory` - will use directory's path


.. _path_command:

path(pathString)
~~~~~~~~~~~~~~~~

Will create a :struct:`Path` structure representing the given path string. You
can omit the argument to create a :struct:`Path` for the current directory.


scriptpath()
~~~~~~~~~~~~

Will return a :struct:`Path` structure representing the path to the currently
running script.

Volumes
-------

volume(volumeIdOrName)
~~~~~~~~~~~~~~~~~~~~~~

Will return a :struct:`Volume` structure representing the volume with a given
id or name. You can omit the argument to create a :struct:`Volume`
for the current volume.

SWITCH TO Volume|volumeId|volumeName.
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

Changes the current directory to the root directory of the specified volume.
Volumes can be referenced by instances of :struct:`Volume`, their ID numbers
or their names if they've been given one. Understanding how
:ref:`volumes work <volumes>` is important to understanding this command.

Example::

    SWITCH TO 0.                        // Switch to volume 0.
    SET VOLUME(1):NAME TO AwesomeDisk.  // Name volume 1 as AwesomeDisk.
    SWITCH TO AwesomeDisk.              // Switch to volume 1.
    PRINT VOLUME:NAME.                  // Prints "AwesomeDisk".


Files and directories
---------------------

.. warning::

    .. versionchanged:: 1.0.0

        **COPY, RENAME and DELETE are now deprecated**

        Previously you could use the aformentioned commands to manipulate files.
        Currently using them will result in a deprecation message being shown.
        After subdirectories were introduced in kOS 1.0 it was necessary to add
        more flexible commands that could deal with both files and directories.
        The old syntax was not designed with directories in mind. It would also
        make it difficult for the kOS parser to properly handle paths.

        Please update your scripts to use the new commands:
        :ref:`movepath(frompath, topath)`, :ref:`copypath(frompath, topath)` and
        :ref:`deletepath(path)`.

LIST
~~~~

Shortcut for :code:`LIST FILES`. Prints the contents (files and directories)
of the current directory.

CD(PATH)
~~~~~~~~

Changes the current directory to the one pointed to by the :code:`PATH`
argument. This command will fail if the path is invalid or does not point
to an existing directory.

COPYPATH(FROMPATH, TOPATH)
~~~~~~~~~~~~~~~~~~~~~~~~~~

Copies the file or directory pointed to by :code:`FROMPATH` to the location
pointed to :code:`TOPATH`. Depending on what kind of items both paths point
to the exact behaviour of this command will differ:

1. :code:`FROMPATH` points to a file

   - :code:`TOPATH` points to a directory

     The file from :code:`FROMPATH` will be copied to the directory.

   - :code:`TOPATH` points to a file

     Contents of the file pointed to by :code:`FROMPATH` will overwrite
     the contents of the file pointed to by :code:`TOPATH`.

   - :code:`TOPATH` points to a non-existing path

     New file will be created at :code:`TOPATH`, along with any parent
     directories if necessary. Its contents will be set to the contents of
     the file pointed to by :code:`FROMPATH`.

2. :code:`FROMPATH` points to a directory

   If :code:`FROMPATH` points to a directory kOS will copy recursively all
   contents of that directory to the target location.

   - :code:`TOPATH` points to a directory

     The directory from :code:`FROMPATH` will be copied inside the
     directory pointed to by :code:`TOPATH`.

   - :code:`TOPATH` points to a file

     The command will fail.

   - :code:`TOPATH` points to a non-existing path

     New directory will be created at :code:`TOPATH`, along with any
     parent directories if necessary. Its contents will be set to the
     contents of the directory pointed to by :code:`FROMPATH`.

3. :code:`FROMPATH` points to a non-existing path

   The command will fail.

MOVEPATH(FROMPATH, TOPATH)
~~~~~~~~~~~~~~~~~~~~~~~~~~

Moves the file or directory pointed to by :code:`FROMPATH` to the location
pointed to :code:`TOPATH`. Depending on what kind of items both paths point
to the exact behaviour of this command will differ, see :code:`COPYPATH` above.

DELETEPATH(PATH)
~~~~~~~~~~~~~~~~

Deleted the file or directory pointed to by :code:`FROMPATH`. Directories are
removed along with all the items they contain.

EXISTS(PATH)
~~~~~~~~~~~~

A shortcut for ``CORE:CURRENTVOLUME:EXISTS(PATH)``. See :meth:`Volume:EXISTS`.

CREATE(PATH)
~~~~~~~~~~~~

A shortcut for ``CORE:CURRENTVOLUME:CREATE(PATH)``. See :meth:`Volume:CREATE`.

CREATEDIR(PATH)
~~~~~~~~~~~~~~~

A shortcut for ``CORE:CURRENTVOLUME:CREATEDIR(PATH)``. See :meth:`Volume:CREATE`.

OPEN(PATH)
~~~~~~~~~~

A shortcut for ``CORE:CURRENTVOLUME:OPEN(PATH)``. See :meth:`Volume:OPEN`.


JSON
----

.. _writejson:

WRITEJSON(OBJECT, PATH)
~~~~~~~~~~~~~~~~~~~~~~~

Serializes the given object to JSON format and saves it under the given path.

Go to :ref:`Serialization page <serialization>` to read more about serialization.

Usage example::

    SET L TO LEXICON().
    SET NESTED TO QUEUE().

    L:ADD("key1", "value1").
    L:ADD("key2", NESTED).

    NESTED:ADD("nestedvalue").

    WRITEJSON(l, "output.json").

READJSON(PATH)
~~~~~~~~~~~~~~

Reads the contents of a file previously created using ``WRITEJSON`` and deserializes them.

Go to :ref:`Serialization page <serialization>` to read more about serialization.

Example::


    SET L TO READJSON("output.json").
    PRINT L["key1"].

Miscellaneous
-------------

.. _run_once:

RUN [ONCE] <PATH>.
~~~~~~~~~~~~~~~~~~

Runs the specified file as a program, optionally passing information to the
program in the form of a comma-separated list of arguments in parentheses.

If the optional ``ONCE`` keyword is used after the word ``RUN``, it means
the run will not actually occur if the program has already been run once
during the current program context.  This is intended mostly for loading library
program files that may have mainline code in them for initialization purposes
that you don't want to get run a second time just because you use the library
in two different subprograms.

``RUN ONCE`` means "Run unless it's already been run, in which case skip it."

.. note::

    *Limitations on file names used for programs*

    All file names used as program names with the ``run`` command must be
    valid identifiers.  They can not contain spaces or special characters. For
    example, you can't have a program named ``this is my-file.ks``.  This rule
    does not necessarily apply to other filenames such as log files.  However
    to use a filename that contains spaces, you will have to put quotes around
    it.

    On case-sensitive filesystems typically found on Linux and Mac, you should
    name program files used with the ``run`` command entirely with
    lowercase-only filenames or the system may fail to find them when you
    use the ``run`` command.

Arguments
^^^^^^^^^

-  <PATH>: File to run.
-  comma-separated-args: a list of values to pass into the program.

Example::

    RUN AutoLaunch.ks.
    RUN AutoLaunch.ksm.
    RUN AutoLaunch.      // runs AutoLaunch.ksm if available, else runs AutoLaunch.ks.
    RUN AutoLaunch( 75000, true, "hello" ).
    RUN AutoLaunch.ks( 75000, true, "hello" ).
    RUN AutoLaunch.ksm( 75000, true, "hello" ).

    RUN ONCE myLibrary. // run myLibrary unless it's been run already.

The program that is reading the arguments sees them in the variables it
mentions in :ref:`DECLARE PARAMETER`.

Important exceptions to the usual filename rules for RUN
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

The RUN command does not allow the same sorts of generic open-ended
filenames that the other
file commands allow. This is very important.

RUN only works when the filename is a bareword filename. It cannot use expression filenames::

    RUN "ProgName"   // THIS WILL FAIL.  Run needs a bareword filename.
    SET ProgName to "MyProgram".
    RUN ProgName     // THIS WILL FAIL also.  It will attempt to run a file
                     // called "ProgName.ksm" or "ProgName.ks", when it sees this,
                     // rather than "MyProgram".

The reasons for the exception to how filenames work for the RUN command are
too complex to go into in large detail here. Here's the short version: While
the kOS system does defer the majority of the work of actually compiling
subprogram scripts until run-time, it still has to generate some header info
about them at compile time, and the filename has to be set in stone at that
time. Changing this would require a large re-write of some of the architecture
of the virtual machine.

LOG TEXT TO PATH
~~~~~~~~~~~~~~~~

Logs the selected text to a file. Can print strings, or the result of an expression.

Arguments
^^^^^^^^^

-  argument 1: Value you would like to log.
-  argument 2: Path pointing to the file to log into.

Example::

    LOG "Hello" to mylog.txt.    // logs to "mylog.txt".
    LOG 4+1 to "mylog" .         // logs to "mylog.ks" because .ks is the default extension.
    LOG "4 times 8 is: " + (4*8) to mylog.   // logs to mylog.ks because .ks is the default extension.


COMPILE PROGRAM (TO COMPILEDPROGRAM)
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

**(experimental)**

Arguments:

    argument 1
        Path to the source file.
    argument 2
        Path to the destination file. If the optional argument 2 is missing, it will assume it's the same as argument 1, but with a file extension changed to ``*.ksm``.

Pre-compiles a script into an :ref:`Kerboscript ML Exceutable
image <compiling>` that can be used
instead of executing the program script directly.

The RUN command (elsewhere on this page) can work with either \*.ks
script files or \*.ksm compiled files.

The full details of this process are long and complex enough to be
placed on a separate page.

Please see :ref:`the details of the Kerboscript ML
Executable <compiling>`.

EDIT PATH
---------

Edits a program pointed to by :code:`PATH`.

Arguments
^^^^^^^^^

-  argument 1: Path of the file for editing.

.. note::

    The Edit feature was lost in version 0.11 but is back again after version
    0.12.2 under a new guise. The new editor is unable to show a monospace
    font for a series of complex reasons involving how Unity works and how
    Squad bundled the KSP game. The editor works, but will be in a proportional
    width font, which isn't ideal for editing code. The best way to edit code
    remains to use a text editor external to KSP, however for a fast peek at
    the code during play, this editor is useful.

Example::

    EDIT filename.       // edits filename.ks
    EDIT filename.ks.    // edits filename.ks
    EDIT "filename.ks".  // edits filename.ks
    EDIT "filename".     // edits filename.ks
    EDIT "filename.txt". // edits filename.txt
