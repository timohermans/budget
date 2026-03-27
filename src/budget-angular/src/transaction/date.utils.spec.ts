import { createDatesPerWeekFor } from './date.utils';

describe('date.utils', () => {
  describe('createDatesPerWeekFor', () => {
    it('should group dates correctly for January 12, 2026', () => {
      // Setup the input date: 2026-01-12
      // Note: Month is 0-indexed, so January is 0
      const inputDate = new Date(2026, 0, 12);

      const result = createDatesPerWeekFor(inputDate);

      // Assertions
      expect(result instanceof Map).toBe(true);
      expect(result.has(1)).toBe(true);
      expect(result.has(2)).toBe(true);
      expect(result.has(3)).toBe(true);
      expect(result.has(4)).toBe(true);
      expect(result.has(5)).toBe(true);

      expect(result.get(1)).toHaveLength(4);
      expect(result.get(1)).toEqual([
        new Date(2026, 0, 1),
        new Date(2026, 0, 2),
        new Date(2026, 0, 3),
        new Date(2026, 0, 4),
      ]);

      expect(result.get(2)).toHaveLength(7);
      expect(result.get(2)).toEqual([
        new Date(2026, 0, 5),
        new Date(2026, 0, 6),
        new Date(2026, 0, 7),
        new Date(2026, 0, 8),
        new Date(2026, 0, 9),
        new Date(2026, 0, 10),
        new Date(2026, 0, 11),
      ]);
      expect(result.get(3)).toHaveLength(7);
      expect(result.get(3)).toEqual([
        new Date(2026, 0, 12),
        new Date(2026, 0, 13),
        new Date(2026, 0, 14),
        new Date(2026, 0, 15),
        new Date(2026, 0, 16),
        new Date(2026, 0, 17),
        new Date(2026, 0, 18),
      ]);
      expect(result.get(4)).toHaveLength(7);
      expect(result.get(4)).toEqual([
        new Date(2026, 0, 19),
        new Date(2026, 0, 20),
        new Date(2026, 0, 21),
        new Date(2026, 0, 22),
        new Date(2026, 0, 23),
        new Date(2026, 0, 24),
        new Date(2026, 0, 25),
      ]);
      expect(result.get(5)).toHaveLength(6);
      expect(result.get(5)).toEqual([
        new Date(2026, 0, 26),
        new Date(2026, 0, 27),
        new Date(2026, 0, 28),
        new Date(2026, 0, 29),
        new Date(2026, 0, 30),
        new Date(2026, 0, 31),
      ]);
    });
  });
});
