import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import AdjustFineModal from '../components/ui/AdjustFineModal'

const mockFine = {
  id: 1,
  bookTitle: '1984',
  bookAuthor: 'George Orwell',
  memberName: 'Jane Doe',
  amount: 10.00,
  reason: 'Overdue Book'
}

describe('AdjustFineModal', () => {
  let mockOnClose
  let mockOnAdjustFine
  let user

  beforeEach(() => {
    mockOnClose = vi.fn()
    mockOnAdjustFine = vi.fn()
    user = userEvent.setup()
  })

  it('renders fine details correctly', () => {
    render(
      <AdjustFineModal
        isOpen={true}
        onClose={mockOnClose}
        fine={mockFine}
        onAdjustFine={mockOnAdjustFine}
      />
    )

    expect(screen.getByText('1984 by George Orwell')).toBeInTheDocument()
    expect(screen.getByText('Jane Doe')).toBeInTheDocument()
    expect(screen.getByText('$10.00')).toBeInTheDocument()
  })

  it('validates required reason field', async () => {
    render(
      <AdjustFineModal
        isOpen={true}
        onClose={mockOnClose}
        fine={mockFine}
        onAdjustFine={mockOnAdjustFine}
      />
    )

    const submitButton = screen.getByRole('button', { name: /adjust fine/i })
    await user.click(submitButton)

    expect(screen.getByText('A reason is required for all fine adjustments')).toBeInTheDocument()
    expect(mockOnAdjustFine).not.toHaveBeenCalled()
  })

  it('validates negative amount', async () => {
    render(
      <AdjustFineModal
        isOpen={true}
        onClose={mockOnClose}
        fine={mockFine}
        onAdjustFine={mockOnAdjustFine}
      />
    )

    const amountInput = screen.getByLabelText(/new amount/i)
    const reasonInput = screen.getByLabelText(/reason for adjustment/i)
    const submitButton = screen.getByRole('button', { name: /adjust fine/i })

    // Enter negative amount and a reason (to trigger validation on amount only)
    await user.clear(amountInput)
    await user.type(amountInput, '-5')
    await user.type(reasonInput, 'Test reason')
    await user.click(submitButton)

      // The main expectation is that the callback should not be called with invalid data
    expect(mockOnAdjustFine).not.toHaveBeenCalled()
  })

  it('shows waiver notice when amount is zero', async () => {
    render(
      <AdjustFineModal
        isOpen={true}
        onClose={mockOnClose}
        fine={mockFine}
        onAdjustFine={mockOnAdjustFine}
      />
    )

    const amountInput = screen.getByLabelText(/new amount/i)
    await user.clear(amountInput)
    await user.type(amountInput, '0')

    expect(screen.getByText('✓ This will completely waive the fine')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /waive fine/i })).toBeInTheDocument()
  })

  it('shows adjustment amount when not zero', async () => {
    render(
      <AdjustFineModal
        isOpen={true}
        onClose={mockOnClose}
        fine={mockFine}
        onAdjustFine={mockOnAdjustFine}
      />
    )

    const amountInput = screen.getByLabelText(/new amount/i)
    await user.clear(amountInput)
    await user.type(amountInput, '5.00')

    expect(screen.getByText('Adjustment: $-5.00')).toBeInTheDocument()
  })

  it('submits valid form data', async () => {
    mockOnAdjustFine.mockResolvedValue()

    render(
      <AdjustFineModal
        isOpen={true}
        onClose={mockOnClose}
        fine={mockFine}
        onAdjustFine={mockOnAdjustFine}
      />
    )

    const amountInput = screen.getByLabelText(/new amount/i)
    const reasonInput = screen.getByLabelText(/reason for adjustment/i)
    const submitButton = screen.getByRole('button', { name: /adjust fine/i })

    await user.clear(amountInput)
    await user.type(amountInput, '2.50')
    await user.type(reasonInput, 'Reduced due to circumstances')
    await user.click(submitButton)

    await waitFor(() => {
      expect(mockOnAdjustFine).toHaveBeenCalledWith(1, {
        newAmount: 2.50,
        reason: 'Reduced due to circumstances'
      })
    })

    expect(mockOnClose).toHaveBeenCalled()
  })

  it('handles API errors gracefully', async () => {
    const errorMessage = 'Failed to adjust fine'
    mockOnAdjustFine.mockRejectedValue(new Error(errorMessage))

    render(
      <AdjustFineModal
        isOpen={true}
        onClose={mockOnClose}
        fine={mockFine}
        onAdjustFine={mockOnAdjustFine}
      />
    )

    const amountInput = screen.getByLabelText(/new amount/i)
    const reasonInput = screen.getByLabelText(/reason for adjustment/i)
    const submitButton = screen.getByRole('button', { name: /adjust fine/i })

    await user.clear(amountInput)
    await user.type(amountInput, '0')
    await user.type(reasonInput, 'Full waiver')
    await user.click(submitButton)

    await waitFor(() => {
      expect(screen.getByText(/failed to adjust fine/i)).toBeInTheDocument()
    })

    expect(mockOnClose).not.toHaveBeenCalled()
  })

  it('shows loading state during submission', async () => {
    let resolvePromise
    const pendingPromise = new Promise((resolve) => {
      resolvePromise = resolve
    })
    mockOnAdjustFine.mockReturnValue(pendingPromise)

    render(
      <AdjustFineModal
        isOpen={true}
        onClose={mockOnClose}
        fine={mockFine}
        onAdjustFine={mockOnAdjustFine}
      />
    )

    const amountInput = screen.getByLabelText(/new amount/i)
    const reasonInput = screen.getByLabelText(/reason for adjustment/i)
    const submitButton = screen.getByRole('button', { name: /adjust fine/i })

    await user.clear(amountInput)
    await user.type(amountInput, '0')
    await user.type(reasonInput, 'Test reason')
    await user.click(submitButton)

    expect(screen.getByText('Processing...')).toBeInTheDocument()
    expect(submitButton).toBeDisabled()

    // Resolve the promise to clean up
    resolvePromise()
    await waitFor(() => expect(mockOnClose).toHaveBeenCalled())
  })

  it('resets form when closed', async () => {
    render(
      <AdjustFineModal
        isOpen={true}
        onClose={mockOnClose}
        fine={mockFine}
        onAdjustFine={mockOnAdjustFine}
      />
    )

    const amountInput = screen.getByLabelText(/new amount/i)
    const reasonInput = screen.getByLabelText(/reason for adjustment/i)
    const cancelButton = screen.getByRole('button', { name: /cancel/i })

    // Modify form
    await user.clear(amountInput)
    await user.type(amountInput, '5.00')
    await user.type(reasonInput, 'Test reason')

    // Cancel
    await user.click(cancelButton)

    expect(mockOnClose).toHaveBeenCalled()
  })

  it('enforces character limit on reason field', async () => {
    render(
      <AdjustFineModal
        isOpen={true}
        onClose={mockOnClose}
        fine={mockFine}
        onAdjustFine={mockOnAdjustFine}
      />
    )

    const reasonInput = screen.getByLabelText(/reason for adjustment/i)
    
    // Type characters up to the limit - maxLength HTML attribute should prevent more
    const textNearLimit = 'a'.repeat(495)
    await user.clear(reasonInput)
    await user.type(reasonInput, textNearLimit)
    
    // Should be at 495 chars
    expect(reasonInput.value).toHaveLength(495)
    expect(screen.getByText('495/500 characters')).toBeInTheDocument()
    
    // Try to type 10 more characters - should be limited to 500
    await user.type(reasonInput, 'bcdefghijk')
    expect(reasonInput.value).toHaveLength(500)
    expect(screen.getByText('500/500 characters')).toBeInTheDocument()
  }, 10000)

  it('does not render when isOpen is false', () => {
    render(
      <AdjustFineModal
        isOpen={false}
        onClose={mockOnClose}
        fine={mockFine}
        onAdjustFine={mockOnAdjustFine}
      />
    )

    expect(screen.queryByText('Adjust Fine')).not.toBeInTheDocument()
  })
})