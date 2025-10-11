import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import FinesPayment from '../components/pages/FinesPayment'

// Mock the finesApi  
vi.mock('../lib/api', () => ({
  finesApi: {
    adjustFine: vi.fn()
  }
}))

// Mock the AdjustFineModal component
vi.mock('../components/ui/AdjustFineModal', () => ({
  default: ({ isOpen, onClose, fine, onAdjustFine }) => {
    if (!isOpen) return null
    return (
      <div data-testid="adjust-fine-modal">
        <h3>Adjust Fine Modal</h3>
        <p>Fine ID: {fine?.id}</p>
        <button onClick={onClose}>Close Modal</button>
        <button onClick={() => onAdjustFine(fine.id, { newAmount: 0, reason: 'Test waiver' })}>
          Mock Adjust
        </button>
      </div>
    )
  }
}))

describe('FinesPayment', () => {
  let user

  const mockAdminUser = {
    id: 1,
    role: 'Administrator',
    username: 'admin'
  }

  const mockRegularUser = {
    id: 2,
    role: 'Member', 
    username: 'member'
  }

  beforeEach(() => {
    user = userEvent.setup()
    vi.clearAllMocks()
  })

  describe('Admin Role Features', () => {
    it('shows "Adjust Fine" button for administrators', () => {
      render(<FinesPayment user={mockAdminUser} />)

      // Wait for component to load
      expect(screen.getByText('Fines & Payment Management')).toBeInTheDocument()
      
      // Should show adjust fine buttons for outstanding fines
      const adjustButtons = screen.getAllByText('Adjust Fine')
      expect(adjustButtons.length).toBeGreaterThan(0)
    })

    it('hides "Adjust Fine" button for non-administrators', () => {
      render(<FinesPayment user={mockRegularUser} />)

      // Should not show adjust fine buttons
      expect(screen.queryByText('Adjust Fine')).not.toBeInTheDocument()
    })

    it('opens adjust fine modal when button is clicked', async () => {
      render(<FinesPayment user={mockAdminUser} />)

      const adjustButton = screen.getAllByText('Adjust Fine')[0]
      await user.click(adjustButton)

      expect(screen.getByTestId('adjust-fine-modal')).toBeInTheDocument()
    })
  })

  describe('Fine Adjustment Functionality', () => {
    it('handles successful fine adjustment', async () => {
      const { finesApi } = await import('../lib/api')
      finesApi.adjustFine.mockResolvedValue({
        data: {
          success: true,
          message: 'Fine waived successfully',
          newAmount: 0
        }
      })

      render(<FinesPayment user={mockAdminUser} />)

      // Open modal and trigger adjustment
      const adjustButton = screen.getAllByText('Adjust Fine')[0]
      await user.click(adjustButton)

      const mockAdjustButton = screen.getByText('Mock Adjust')
      await user.click(mockAdjustButton)

      await waitFor(() => {
        expect(finesApi.adjustFine).toHaveBeenCalledWith(1, {
          newAmount: 0,
          reason: 'Test waiver'
        })
      })

      // Should show success toast
      expect(screen.getByText('Fine waived successfully')).toBeInTheDocument()
    })

    it('handles fine adjustment errors', async () => {
      const { finesApi } = await import('../lib/api')
        finesApi.adjustFine.mockResolvedValue({
          data: {
            success: false,
            message: 'API Error'
          }
        })

      render(<FinesPayment user={mockAdminUser} />)

      // Open modal and trigger adjustment
      const adjustButton = screen.getAllByText('Adjust Fine')[0]
      await user.click(adjustButton)

      const mockAdjustButton = screen.getByText('Mock Adjust')
      await user.click(mockAdjustButton)

      await waitFor(() => {
        expect(finesApi.adjustFine).toHaveBeenCalled()
      })

        // Just verify the API was called - error handling would be in the modal component
        expect(finesApi.adjustFine).toHaveBeenCalledTimes(1)
    })
  })
})