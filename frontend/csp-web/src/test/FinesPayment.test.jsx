import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import FinesPayment from '../../components/pages/FinesPayment'
import * as api from '../../lib/api'

const userMember = { id: 1, role: 'Member' }
const userAdmin = { id: 2, role: 'Administrator' }

describe('FinesPayment', () => {
  beforeEach(() => {
    vi.restoreAllMocks()
  })

  it('renders outstanding fines for member from backend', async () => {
    vi.spyOn(api.lendingApi, 'getActiveLoans').mockResolvedValue({
      data: {
        items: [
          { id: 10, bookTitle: 'Book A', bookAuthor: 'Auth', status: 'Overdue', dueDate: '2025-10-01', fineAmount: 1.5, finePaid: false, overdueDays: 6 },
          { id: 11, bookTitle: 'Book B', bookAuthor: 'Auth', status: 'Active', dueDate: '2025-10-10', fineAmount: 0, finePaid: false }
        ]
      }
    })

    render(<FinesPayment user={userMember} />)

    expect(await screen.findByText('My Fines & Payments')).toBeInTheDocument()
    await waitFor(() => {
      expect(screen.getByText('Outstanding Fines')).toBeInTheDocument()
      expect(screen.getByText('Book A')).toBeInTheDocument()
      expect(screen.getByText('$1.50')).toBeInTheDocument()
    })
  })

  it('falls back to mock data when backend fails', async () => {
    vi.spyOn(api.lendingApi, 'getActiveLoans').mockRejectedValue(new Error('network'))

    render(<FinesPayment user={userMember} />)

    expect(await screen.findByText('My Fines & Payments')).toBeInTheDocument()
    await waitFor(() => {
      expect(screen.getByText('To Kill a Mockingbird')).toBeInTheDocument()
      expect(screen.getByText('$3.50')).toBeInTheDocument()
    })
  })

  it('shows admin labels and actions for staff', async () => {
    vi.spyOn(api.lendingApi, 'getActiveLoans').mockResolvedValue({ data: { items: [] } })

    render(<FinesPayment user={userAdmin} />)

    expect(await screen.findByText('Fines & Payment Management')).toBeInTheDocument()
    expect(screen.getByText('All Fines')).toBeInTheDocument()
    await waitFor(() => {
      expect(screen.getByText('No fines to manage at this time.')).toBeInTheDocument()
    })
  })
})
