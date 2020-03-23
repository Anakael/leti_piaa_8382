#!/bin/bash

FILES=Tests/*
for test in $FILES
do
  echo "Processing $test file:"
  echo "Input data:"
  cat $test
  echo "Output data:"
  ./lab1 < $test
done
