#!/bin/bash

## Copyright (c) Improbable Worlds Ltd, All Rights Reserved
## ===========

## Name: fix_xcode_5_6.sh
## Description: Fixes invalid IL2CPP-generated Xcode source code 
##                 for SpatialOS Unity projects. 
# --------------------------------------------------------------------------

# Utility function 
# Insert a string into a specific line of a file.
# --------------------------------------------------------------------------
function InsertAtLineForFile {
  # args:
  #  -- $1: line number
  #  -- $2: string to insert
  #  -- $3: filename
  printf '%s\n' H $1i "$2" . wq | ed -s $3
}


# Utility Function
# Insert contents of file $1 into file $2 at line $3.
# --------------------------------------------------------------------------
insert_at () { insert="$1" ; into="$2" ; at="$3" ; { head -n $at "$into" ; ((at++)) ; cat "$insert" ; tail -n +$at "$into" ; } ; }
insert_at_replace () { tmp=$(mktemp) ; insert_at "$@" > "$tmp" ; mv "$tmp" "$2" ; }


# Search for the location of the affected files
# --------------------------------------------------------------------------
echo "Searching for IL2CPP generated .h file of: Improbable.Worker.Internal.WorkerProtocol/Constraint/Union"
union_filename=$(find build/worker/UnityClient@iOS/UnityClient@iOS/Classes/Native/ -type f -print | xargs grep -l "struct  Union_t[0-9]\+\s*$")


echo "Searching for IL2CPP generated .h file of: Improbable.Worker.Internal.WorkerProtocol/AndConstraint"
and_constraint_filename=$(find build/worker/UnityClient@iOS/UnityClient@iOS/Classes/Native/ -type f -print | xargs grep -l "struct  AndConstraint_t[0-9]\+\s*$")



# Add blocks around: Improbable.Worker.Internal.WorkerProtocol/AndConstraint
# --------------------------------------------------------------------------
echo "Fixing Improbable.Worker.Internal.WorkerProtocol/AndConstraint ..."
if ! grep -Fxq "#define __io__andconstraint__" ${and_constraint_filename} ; then
    line_number=$(awk '/Improbable.Worker.Internal.WorkerProtocol\/AndConstraint/{ print NR; exit }' ${and_constraint_filename})
    InsertAtLineForFile ${line_number} '#define __io__andconstraint__' ${and_constraint_filename}
    InsertAtLineForFile ${line_number} '#ifndef __io__andconstraint__' ${and_constraint_filename}
    echo "#endif" >> ${and_constraint_filename}
    echo "   > ... successfully fixed."
else
    echo "   > ... was fixed previously."
fi


# Insert AndConstraint definition into
# Improbable.Worker.Internal.WorkerProtocol/Constraint/Union
# --------------------------------------------------------------------------
echo "Fixing Improbable.Worker.Internal.WorkerProtocol/Constraint/Union ..."
if ! grep -Fxq "#define __io__andconstraint__" ${union_filename} ; then
    line_end=$(wc -l < "${and_constraint_filename}")
    cat ${and_constraint_filename} | tail -$(($line_end - $line_number + 1)) > "and_tmp"
    echo "" >> "and_tmp"
    target_line=$(awk '/Improbable.Worker.Internal.WorkerProtocol\/Constraint\/Union/{ print NR; exit }' ${union_filename})
    insert_at_replace "and_tmp" ${union_filename} ${target_line}
    echo "   > ... required AndConstraint successfully fixed."
    rm "and_tmp"
else
    echo "   > ... required AndConstraint was fixed previously."
fi


# Fixing missing includes in Il2CppReversePInvokeWrapperTable.cpp
# --------------------------------------------------------------------------
missing_includes='#include "Improbable_WorkerSdkCsharp_Improbable_Worker_Intern557692849.h"
#include "Improbable_WorkerSdkCsharp_Improbable_Worker_Interna77883717.h"
#include "Improbable_WorkerSdkCsharp_Improbable_Worker_Intern954583219.h"
#include "Improbable_WorkerSdkCsharp_Improbable_Worker_Inter2102163222.h"
#include "Improbable_WorkerSdkCsharp_Improbable_Worker_Inter2392035476.h"
#include "Improbable_WorkerSdkCsharp_Improbable_Worker_Inter1827422924.h"
#include "Improbable_WorkerSdkCsharp_Improbable_Worker_Inter1173538928.h"
#include "Improbable_WorkerSdkCsharp_Improbable_Worker_Inter2349150323.h"
#include "Improbable_WorkerSdkCsharp_Improbable_Worker_Inter3671131290.h"
#include "Improbable_WorkerSdkCsharp_Improbable_Worker_Inter1234217516.h"
#include "Improbable_WorkerSdkCsharp_Improbable_Worker_Inter3202639533.h"
#include "Improbable_WorkerSdkCsharp_Improbable_Worker_Intern619885720.h"
#include "Improbable_WorkerSdkCsharp_Improbable_Worker_Inter2927579602.h"
#include "Improbable_WorkerSdkCsharp_Improbable_Worker_Inter2833004525.h"
#include "Improbable_WorkerSdkCsharp_Improbable_Worker_Intern960296935.h"
#include "Improbable_WorkerSdkCsharp_Improbable_Worker_Intern780823014.h"
#include "Improbable_WorkerSdkCsharp_Improbable_Worker_Inter2592032334.h"
#include "Improbable_WorkerSdkCsharp_Improbable_Worker_Intern224259713.h"
#include "Improbable_WorkerSdkCsharp_Improbable_Worker_Inter2948868343.h"
#include "Improbable_WorkerSdkCsharp_Improbable_Worker_Inter2330876060.h"
#include "Improbable_WorkerSdkCsharp_Improbable_Worker_Inter1490226248.h"
#include "Improbable_WorkerSdkCsharp_Improbable_Worker_Inter1716928168.h"'  

echo "Fixing Il2CppReversePInvokeWrapperTable..."
if ! grep -Fxq "${missing_includes}" "build/worker/UnityClient@iOS/UnityClient@iOS/Classes/Native/Il2CppReversePInvokeWrapperTable.cpp" ; then
	InsertAtLineForFile 13 "${missing_includes}" "build/worker/UnityClient@iOS/UnityClient@iOS/Classes/Native/Il2CppReversePInvokeWrapperTable.cpp"
    echo "   > ... Includes were fixed"
else
    echo "   > ... required includes were fixed previously."
fi

