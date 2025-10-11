import { describe, it, expect, vi, beforeEach } from 'vitest'

describe.sequential('finesApi', () => {
  let mockApi
  let finesApi

  beforeEach(async () => {
    vi.clearAllMocks()
    vi.resetModules()
    
    // Create mock axios instance
    mockApi = {
      put: vi.fn(),
      interceptors: {
        request: { use: vi.fn() },
        response: { use: vi.fn() }
      }
    }
    
    // Mock axios module
    vi.doMock('axios', () => ({
      default: {
        create: vi.fn().mockReturnValue(mockApi),
      }
    }))
    
    // Import the module after mocking
    const apiModule = await import('../lib/api')
    finesApi = apiModule.finesApi
  })

  describe('adjustFine', () => {
    it('calls PUT endpoint with correct parameters', async () => {
      const lendingId = 123
      const adjustmentData = {
        newAmount: 5.50,
        reason: 'Reduced due to special circumstances'
      }

      mockApi.put.mockResolvedValue({
        data: {
          success: true,
          message: 'Fine updated successfully'
        }
      })

      const result = await finesApi.adjustFine(lendingId, adjustmentData)

      expect(mockApi.put).toHaveBeenCalledWith(
        `/fines/${lendingId}/adjust`,
        adjustmentData
      )
      expect(result.data.success).toBe(true)
    })

    it('handles successful waiver response', async () => {
      mockApi.put.mockResolvedValue({
        data: {
          success: true,
            message: 'Fine updated successfully'
        }
      })

      const result = await finesApi.adjustFine(123, {
        newAmount: 0,
        reason: 'Full waiver approved'
      })

      expect(result.data.success).toBe(true)
        expect(result.data.message).toBe('Fine updated successfully')
    })

    it('returns response data correctly', async () => {
      const expectedData = {
        success: true,
        message: 'Fine updated successfully',
        updatedFine: 10.00
      }
      
      mockApi.put.mockResolvedValue({ data: expectedData })

      const result = await finesApi.adjustFine(123, {
        newAmount: 10.00,
        reason: 'Test adjustment'
      })

      expect(result.data).toEqual(expectedData)
    })
  })
})