import PageHeader from '../shared/components/layout/PageHeader';

export default function PlaceholderPage({ title }: { title: string }) {
  return (
    <div>
      <PageHeader title={title} subtitle="هذه الصفحة قيد البناء بالدور القادم" />
    </div>
  );
}
