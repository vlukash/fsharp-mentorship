module MandelbrotTypes

type Line = int * byte array

type ComputationType =
    | SingleThreadType
    | AkkaNETType

// ix tx ty mx my maxIter imageWidth
type ParallelTask = int * float * float * float * float * int * int

// ix byte array
type ParallelTaskResult = int * byte array