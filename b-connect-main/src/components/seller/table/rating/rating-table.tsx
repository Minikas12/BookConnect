import { Button } from 'src/components/ui/button'
import { DataTable } from 'src/components/ui/data-table'
import React, { useEffect, useMemo, useState } from 'react'
import Pagination from 'src/components/ui/pagination'
import TableSizeSelector from 'src/components/ui/table-size-selector'
import { Skeleton } from 'src/components/ui/skeleton'
import { columns } from './column'
import { RatingTableToolbar } from './toolbar'
import { useRatingTable } from './useRatingTable'

function RatingTable() {
  const { isError, isLoading, table, error, refetch, data, tableStates, setTableStates, queries, setQueries } =
    useRatingTable(columns)
  const [day, setDay] = useState<string>()
  const [isRep, setIsRep] = useState<string>()
  const handleRecentDaysChange = (value: string) => {
    setDay(value)
  }

  const handleHasRepliedChange = (value: string) => {
    setIsRep(value)
  }

  useEffect(() => {
    setTableStates((prev) => ({
      ...prev,
      recentDays: day,
    }))
  }, [day, setTableStates])

  useEffect(() => {
    setTableStates((prev) => ({
      ...prev,
      hasReplied: isRep,
    }))
  }, [isRep, setTableStates])

  const renderHeader = useMemo(() => {
    return (
      <div>
        <RatingTableToolbar
          table={table}
          queries={{
            PageNumber: tableStates.pagination.pageIndex + 1,
            PageSize: tableStates.pagination.pageSize,
            BookName: tableStates.globalFilter,
            // RecentDays: day,
            // HasReplied: isRep,
            ...queries,
          }}
          setSearchQuery={(value) => {
            setQueries(value)
            table.setGlobalFilter(value.BookName),
              table.setColumnFilters(() => [
                {
                  id: 'ratingPoint',
                  value: value.RatingPoint,
                },
              ])
          }}
          onRecentDaysChange={handleRecentDaysChange}
          onHasRepliedChange={handleHasRepliedChange}
        />
      </div>
    )
  }, [day, isRep, table, tableStates.pagination.pageIndex, tableStates.pagination.pageSize, tableStates.globalFilter])

  const renderFooter = React.useMemo(() => {
    if (isLoading)
      return (
        <div className="flex justify-end gap-2 px-3 py-1.5">
          <Skeleton className="h-8 w-20" />
          <Skeleton className="h-8 w-20" />
        </div>
      )
    return (
      <>
        <div className="flex justify-end gap-2 px-3 py-1.5">
          <TableSizeSelector
            className="right-0 max-w-[100px]"
            defaultSize={table.getState().pagination.pageSize}
            onChange={(value) => {
              table.setPageSize(value)
            }}
          />
          <Pagination
            currentPage={tableStates.pagination.pageIndex + 1}
            totalPage={data?._pagination?.TotalPages || 1}
            onPageChange={(index) => {
              table.setPageIndex(index - 1)
            }}
            onFirstPage={() => table.setPageIndex(0)}
            onNextPage={() => {
              table.nextPage()
            }}
            onPreviousPage={() => {
              table.previousPage()
            }}
            onLastPage={() => table.setPageIndex(table.getPageCount() - 1)}
          />
        </div>
      </>
    )
  }, [isLoading, tableStates.pagination.pageIndex, table, data])

  return (
    <div className="mt-4">
      {isError && <Button onClick={() => refetch()}>Retry</Button>}
      {isError && <p>{error?.message}</p>}
      <DataTable
        table={table}
        isLoading={isLoading}
        header={renderHeader}
        columns={columns}
        data={data?.data ?? []}
        footer={renderFooter}
      />
    </div>
  )
}
export default RatingTable
