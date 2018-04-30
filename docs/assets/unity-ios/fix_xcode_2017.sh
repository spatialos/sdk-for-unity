#!/bin/bash

## Copyright (c) Improbable Worlds Ltd, All Rights Reserved
## ===========

## Name: fix_xcode_2017.sh
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

cd build/worker/UnityClient@iOS/UnityClient@iOS/

single_quote="'"
last_missing_type=""
while true; do
	echo "Building Xcode project..."
	build_errors=$(xcodebuild build)
	missing_types=($( echo "${build_errors}" | egrep -o "/Il2CppReversePInvokeWrapperTable.cpp.* error: unknown type name '.*?'" | awk '{ print $NF }' | sort | uniq))
	if [ "${#missing_types[@]}" -eq "0" ]; then
		echo "Fixed all 'unknown type name' errors!"
		exit 0
	fi

	echo "Fixing 'unknown type name' errors in Il2CppReversePInvokeWrapperTable.cpp"
	for missing_type in ${missing_types[@]}; do
		stripped_type=$(echo "${missing_type}" | tr -d "'" | tr /a-z/ /A-Z/)
		echo ${stripped_type}
		grepped_definition=$(pcregrep -Mr "#ifndef ${stripped_type}_H[\s\S]+#endif \/\/ ${stripped_type}_H" Classes/Native/Bulk_Improbable.WorkerSdkCsharp_0.cpp)
		InsertAtLineForFile 13 "
		${grepped_definition}" "Classes/Native/Il2CppReversePInvokeWrapperTable.cpp"
	done
	echo ""
done
