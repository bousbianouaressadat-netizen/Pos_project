import { forwardRef } from 'react';
import './ProductSearch.css';

interface ProductSearchProps {
  value: string;
  onChange: (value: string) => void;
  onBarcodeSubmit: (code: string) => void;
}

const ProductSearch = forwardRef<HTMLInputElement, ProductSearchProps>(
  ({ value, onChange, onBarcodeSubmit }, ref) => {
    return (
      <input
        ref={ref}
        type="text"
        className="product-search"
        placeholder="ابحث بالاسم أو امسح الباركود... (F2)"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        onKeyDown={(e) => {
          if (e.key === 'Enter' && value.trim()) {
            onBarcodeSubmit(value.trim());
          }
        }}
        autoComplete="off"
      />
    );
  }
);

ProductSearch.displayName = 'ProductSearch';
export default ProductSearch;
