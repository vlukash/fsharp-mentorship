module MandelbrotTypes

type Line = int * byte array

type ComputationType =
    | SingleThreadType
    | AkkaNETType
    | ParallelForGPUType

// ix tx ty mx my maxIter imageWidth
type ParallelTask = int * float * float * float * float * int * int

// Block of data to be calculated on any worker
type ParallelTasksBlock = ParallelTask list

// ix byte array
type ParallelTaskResult = int * byte array